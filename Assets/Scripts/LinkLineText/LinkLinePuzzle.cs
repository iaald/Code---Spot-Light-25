using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public partial class LinkLinePuzzle : MonoBehaviour
{
    public bool freezed;
    public Camera eventCamera;
    public LayerMask occluderMask = ~0; // 射线遮挡层（默认命中所有物理层）
    public bool enableUIOccluder = true; // 是否考虑 UI 作为遮挡体
    public bool debugRaycast = false;     // 调试信息开关
    public float debugLogInterval = 0.2f; // 调试日志节流
    private float _dbgNextLogTime = 0f;
    [Header("Sprites")]
    public Sprite squareSprite;    // 未涂色
    public Sprite arrowSprite;     // 线体内部箭头
    public Sprite endpointSprite;  // 活动端点

    [Header("Board Size")]
    public int rows = 4;
    public int cols = 4;

    [Header("Colors")]
    public Color inactiveColor = Color.white;
    public Color activeColor = Color.green;

    // —— 三张表（全局共享） —— //
    // 线体：+lineId，端点：-lineId，空：0
    private int[,] ownerMap;
    // 是否曾被上色（用于“首次上色序列”）
    private int[,] activeMap;
    // 进出方向（NESW：1,2,4,8）
    private byte[,] dirMap;

    // block 映射
    private readonly Dictionary<Vector2Int, LinkLineTextBlock> pos2Block = new();
    private readonly Dictionary<LinkLineTextBlock, Vector2Int> block2Pos = new();

    // 鼠标所在格（不在格上为 -1,-1）
    private Vector2Int mouseGrid = new(-1, -1);

    // —— 线状态 —— //
    private class LineState
    {
        public int lineId;
        public Vector2Int start;
        public Vector2Int end;
        public Stack<Vector2Int> path = new(); // start -> ... -> end（栈顶为端点）
    }

    private LineState currentLine;           // 活动中的线（仅一条）
    private readonly List<LineState> committedLines = new(); // 已冻结的线

    private int nextLineId = 1; // 自增 id（确保不同起点有不同标记）

    // 全局“首次上色”的序列（不含被回擦；若末尾被回擦则从尾部回退）
    private readonly List<int> sequence = new();

    // 同步
    private int expectedBlocks;
    private int registeredBlocks;
    private bool boardReady;

    // 方向常量
    private static readonly Vector2Int[] DIRS = {
        new(-1, 0), // N
        new(0, 1),  // E
        new(1, 0),  // S
        new(0, -1)  // W
    };
    private const byte N = 1, E = 2, S = 4, W = 8;

    void Awake()
    {
        expectedBlocks = transform.childCount;
        ownerMap = new int[rows, cols];
        activeMap = new int[rows, cols];
        dirMap = new byte[rows, cols];
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => registeredBlocks >= expectedBlocks);
        boardReady = true;
        foreach (var kv in pos2Block)
        {
            kv.Value.SetColor(inactiveColor);
            if (squareSprite != null) kv.Value.image.sprite = squareSprite;
            kv.Value.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
       
        if (!boardReady) return;

        if (freezed)
        {
            return;
        }

        // 定位鼠标所在格
        mouseGrid = FindMouseGrid();

        var mouse = Mouse.current;
        if (mouse == null) return;

        // 按下：尝试开始一条新线（仅当起点空闲）
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryBeginCurrentLine(mouseGrid);
        }

        // 按住：只驱动当前线的端点
        if (mouse.leftButton.isPressed && currentLine != null && mouseGrid.x >= 0)
        {
            StepCurrentLineTowards(mouseGrid);
        }

        // 松开：冻结当前线，不可再改
        if (mouse.leftButton.wasReleasedThisFrame && currentLine != null)
        {
            FreezeCurrentLine();
            // TODO 检测并删除非法的线
        }
    }

    // —— 注册 —— //
    public void RegisterBlock(LinkLineTextBlock block, int row, int col)
    {
        var p = new Vector2Int(row, col);
        if (!pos2Block.ContainsKey(p))
        {
            pos2Block[p] = block;
            block2Pos[block] = p;
            registeredBlocks++;
        }
    }

    // —— 序列 —— //
    public List<int> GetSequence() => new(sequence);

    [ContextMenu("Print Sequence")]
    void DebugPrint() => Debug.Log("Current Sequence: " + string.Join(", ", sequence));

    // —— 开始一条活动线 —— //
    private void TryBeginCurrentLine(Vector2Int g)
    {
        if (!IsInside(g)) return;
        if (ownerMap[g.x, g.y] != 0) return; // 3.3：起点必须空

        currentLine = new LineState
        {
            lineId = nextLineId++,
            start = g,
            end = g
        };
        currentLine.path.Push(g);

        ownerMap[g.x, g.y] = -currentLine.lineId; // 端点
        ActivateCell(g);
        SetEndpointAt(g);
    }

    // —— 推进当前线 —— //
    private void StepCurrentLineTowards(Vector2Int target)
    {
        var line = currentLine;
        if (line == null) return;

        var end = line.end;
        if (end == target) return;                 // 3.4：未移动
        if (!IsNeighbor(end, target)) return;      // 仅 4 邻接

        int mark = ownerMap[target.x, target.y];

        // 3.1 回擦：目标在当前线体/路径上（同标记 +id 或 path 包含）
        if (mark == line.lineId || line.path.Contains(target))
        {
            BacktrackTo(line, target);
            return;
        }


        // 3.2 画线：目标无标记
        if (mark == 0)
        {
            // 旧端点：-id -> +id
            ownerMap[end.x, end.y] = line.lineId;

            pos2Block[new(end.x, end.y)].transform.localRotation = Quaternion.Euler(0, 0, 0);
            // 写双向方向位
            WriteDir(end, target, add: true);

            SetArrowForSegment(end, target);

            // 新端点：-id
            line.end = target;
            line.path.Push(target);
            ownerMap[target.x, target.y] = -line.lineId;

            ActivateCell(target);
            SetEndpointAt(target);
            return;
        }

        // 3.3 异色：已有其它线/冻结内容，忽略
    }

    // —— 回擦到 target（仅对当前未冻结线生效） —— //
    private void BacktrackTo(LineState line, Vector2Int target)
    {
        while (line.path.Count > 0 && line.path.Peek() != target)
        {
            var cur = line.path.Pop();    // 被回擦
            var prev = line.path.Peek();   // 新端点候选

            // 移除 prev<->cur 的方向
            WriteDir(prev, cur, add: false);

            // 清理 cur（owner/dir/active/颜色）
            ClearCell(cur);

            // prev 成为端点：-id
            ownerMap[prev.x, prev.y] = -line.lineId;
            SetEndpointAt(prev);
        }

        line.end = target;
    }

    // —— 冻结当前线（鼠标松开） —— //
    private void FreezeCurrentLine()
    {
        if (currentLine == null) return;

        // 如果松开时回到了起点（仅剩一个点）：整条擦除，不保留到 committedLines
        if (currentLine.path.Count == 1 && currentLine.path.Peek() == currentLine.start)
        {
            // 清除起点
            ClearCell(currentLine.start);
            ownerMap[currentLine.start.x, currentLine.start.y] = 0;
            currentLine = null;
            return;
        }

        // 否则正常冻结：把端点 -id -> +id，并加入 committedLines
        var e = currentLine.end;
        if (IsInside(e) && ownerMap[e.x, e.y] == -currentLine.lineId)
            ownerMap[e.x, e.y] = currentLine.lineId;
        committedLines.Add(currentLine);

        if (!CheckLine(currentLine.lineId))
        {
            DeleteCommittedLine(currentLine.lineId);
        }
        else
        {
            var groupMat = pos2Block[currentLine.path.Peek()].GetClonedRuntimeMaterial();
            foreach (var item in currentLine.path)
            {
                var temp = pos2Block[item];
                temp.FillColor(groupMat);
                temp.image.material = groupMat;
            }
        }

        currentLine = null;
        // Check if is solved
        CheckPuzzle();
    }

    // —— 激活/清理 与 “首次上色序列” —— //
    private void ActivateCell(Vector2Int g)
    {
        if (activeMap[g.x, g.y] == 0)
        {
            activeMap[g.x, g.y] = 1;
            int seqVal = pos2Block[g].Seq;
            if (!sequence.Contains(seqVal))
                sequence.Add(seqVal); // 仅首次上色进入序列
        }
        pos2Block[g].SetColor(ColorHelper.ColorRotateGeneration(activeColor, committedLines.Count * 48.75f, -0.2f));
    }

    private void ClearCell(Vector2Int g)
    {
        ownerMap[g.x, g.y] = 0;
        dirMap[g.x, g.y] = 0;

        // 清理
        SetSquareAt(g);

        // 若末尾被回擦，允许从尾部回退序列
        int seqVal = pos2Block[g].Seq;
        if (sequence.Count > 0 && sequence[^1] == seqVal)
            sequence.RemoveAt(sequence.Count - 1);

        activeMap[g.x, g.y] = 0;
    }

    // —— dirMap 工具 —— //
    private void WriteDir(Vector2Int a, Vector2Int b, bool add)
    {
        var d = new Vector2Int(b.x - a.x, b.y - a.y);
        byte abit = 0, bbit = 0;
        if (d == DIRS[0]) { abit = N; bbit = S; }
        else if (d == DIRS[1]) { abit = E; bbit = W; }
        else if (d == DIRS[2]) { abit = S; bbit = N; }
        else if (d == DIRS[3]) { abit = W; bbit = E; }
        else return;

        if (add)
        {
            dirMap[a.x, a.y] |= abit;
            dirMap[b.x, b.y] |= bbit;
        }
        else
        {
            dirMap[a.x, a.y] = (byte)(dirMap[a.x, a.y] & ~abit);
            dirMap[b.x, b.y] = (byte)(dirMap[b.x, b.y] & ~bbit);
        }
    }
    public List<Vector2Int> GetOutDirections(Vector2Int grid)
    {
        var dirs = new List<Vector2Int>();
        if (!IsInside(grid)) return dirs;

        byte mask = dirMap[grid.x, grid.y];
        if ((mask & N) != 0) dirs.Add(new Vector2Int(-1, 0)); // 上
        if ((mask & E) != 0) dirs.Add(new Vector2Int(0, 1));  // 右
        if ((mask & S) != 0) dirs.Add(new Vector2Int(1, 0));  // 下
        if ((mask & W) != 0) dirs.Add(new Vector2Int(0, -1)); // 左

        return dirs;
    }
    // —— 从某条“已冻结线”的起点遍历得到经过顺序（可选） —— //
    public List<string> GetSequenceFromCommittedLineStart(int lineId)
    {
        var line = committedLines.FirstOrDefault(l => l.lineId == lineId);
        if (line == null) return new();

        var order = new List<string>();
        var visited = new HashSet<Vector2Int>();

        var cur = line.start;
        Vector2Int prev = new(int.MinValue, int.MinValue);

        while (true)
        {
            if (!visited.Add(cur)) break;
            order.Add(pos2Block[cur].Text);

            bool moved = false;
            for (int k = 0; k < 4; k++)
            {
                var d = DIRS[k];
                var nxt = cur + d;
                if (!IsInside(nxt)) continue;
                if ((dirMap[cur.x, cur.y] & BitOf(d)) == 0) continue;
                if (nxt == prev) continue;

                prev = cur;
                cur = nxt;
                moved = true;
                break;
            }
            if (!moved) break;
        }
        return order;
    }

    /// <summary>
    /// Delete a committed line by its lineId.
    /// Returns true if found and removed; false if no such committed line exists.
    /// </summary>
    public bool DeleteCommittedLine(int lineId)
    {
        // 只在 committedLines 中查找，当前正在绘制的线不在删除范围
        int idx = committedLines.FindIndex(l => l.lineId == lineId);
        if (idx < 0) return false;

        var line = committedLines[idx];

        // 取路径数组：Stack 的枚举顺序是从栈顶（端点）到栈底（起点），我们反转成 起点->...->端点
        var cells = line.path.ToArray();
        System.Array.Reverse(cells); // 现在是 start -> ... -> end

        // 1) 先移除这条线各段在 dirMap 上的双向连接
        for (int i = 0; i < cells.Length - 1; i++)
        {
            WriteDir(cells[i], cells[i + 1], add: false);
        }

        // 2) 再逐格清理。为了让全局 sequence 能“从尾部回退”，
        //    我们按 端点->...->起点 的逆序清（与首次上色的顺序相反）
        for (int i = cells.Length - 1; i >= 0; i--)
        {
            ClearCell(cells[i]);
        }

        // 3) 从已提交列表中移除
        committedLines.RemoveAt(idx);

        return true;
    }

    private byte BitOf(Vector2Int d)
    {
        if (d == DIRS[0]) return N;
        if (d == DIRS[1]) return E;
        if (d == DIRS[2]) return S;
        if (d == DIRS[3]) return W;
        return 0;
    }

    private void SetEndpointAt(Vector2Int cell)
    {
        var b = pos2Block[cell];
        if (endpointSprite != null) b.image.sprite = endpointSprite;
        b.transform.localRotation = Quaternion.identity; // 端点不需要方向
    }

    // 把 from 这一格的箭头旋成指向 to 的角度
    private void SetArrowForSegment(Vector2Int from, Vector2Int to)
    {
        var d = new Vector2Int(to.x - from.x, to.y - from.y);
        float z = 0f; // 0=向上
        if (d.x == -1 && d.y == 0) z = 0f;    // 北：向上
        else if (d.x == 1 && d.y == 0) z = 180f;  // 南：向下
        else if (d.x == 0 && d.y == 1) z = -90f;  // 东：向右
        else if (d.x == 0 && d.y == -1) z = 90f;   // 西：向左

        var b = pos2Block[from];
        if (arrowSprite != null) b.image.sprite = arrowSprite;
        b.image.transform.localRotation = Quaternion.Euler(0, 0, z);
    }

    // 端点不应有“出方向”箭头：把该格箭头清掉（这里只恢复到一个中性角度）；
    // 如果你有单独的箭头子节点，可以在这里直接禁用它）
    private void ClearArrowAt(Vector2Int cell)
    {
        pos2Block[cell].transform.localRotation = Quaternion.Euler(0, 0, 0);
        // 若有单独箭头 Image：pos2Block[cell].arrowImage.enabled = false;
    }

    private void SetSquareAt(Vector2Int cell)
    {
        var b = pos2Block[cell];
        if (squareSprite != null) b.image.sprite = squareSprite;
        b.transform.localRotation = Quaternion.identity;
        b.SetColor(inactiveColor);
    }

    // —— 工具 —— //
    private bool IsInside(Vector2Int g) => (g.x >= 0 && g.x < rows && g.y >= 0 && g.y < cols);

    private bool IsNeighbor(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    private Vector2Int FindMouseGrid()
    {
        if (Mouse.current == null) return new(-1, -1);
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 统一使用同一相机参与 UI 命中和世界射线
        var cam = eventCamera != null ? eventCamera : Camera.main;

        // UI 遮挡优先：若顶层 UI 命中且非本 Grid 的块，则直接视为被遮挡
        if (enableUIOccluder && EventSystem.current != null)
        {
            var ped = new PointerEventData(EventSystem.current) { position = mousePos };
            var uiHits = new List<RaycastResult>(8);
            EventSystem.current.RaycastAll(ped, uiHits);
            if (uiHits.Count > 0)
            {
                var top = uiHits[0];
                var topBlock = top.gameObject.GetComponentInParent<LinkLineTextBlock>();
                if (topBlock == null)
                {
                    if (debugRaycast && Time.unscaledTime >= _dbgNextLogTime)
                    {
                        Debug.Log($"[LinkLinePuzzle] UI 顶层遮挡: {top.gameObject.name} @dist={top.distance:F3}");
                        _dbgNextLogTime = Time.unscaledTime + debugLogInterval;
                    }
                    return new(-1, -1);
                }
            }
        }

        // 无可用相机（如 Overlay Canvas 且未指定 eventCamera）时回退老逻辑
        if (cam == null)
        {
            foreach (var kv in pos2Block)
            {
                var rt0 = kv.Value.Rect;
                if (RectTransformUtility.RectangleContainsScreenPoint(rt0, mousePos, cam))
                    return kv.Key;
            }
            return new(-1, -1);
        }

        // 物理遮挡：从相机发射射线，检测最近命中体
        var ray = cam.ScreenPointToRay(mousePos);
        float firstHitDistance = float.PositiveInfinity;
        RaycastHit firstHit;
        bool hasHit = Physics.Raycast(ray, out firstHit, Mathf.Infinity, occluderMask, QueryTriggerInteraction.Ignore);
        if (hasHit)
        {
            firstHitDistance = firstHit.distance;
        }
        if (debugRaycast)
        {
            var len = float.IsInfinity(firstHitDistance) ? 100f : firstHitDistance;
            Debug.DrawRay(ray.origin, ray.direction * len, hasHit ? Color.red : Color.yellow, 0f);
            if (hasHit && Time.unscaledTime >= _dbgNextLogTime)
            {
                Debug.Log($"[LinkLinePuzzle] 物理首碰: {firstHit.collider.gameObject.name} layer={firstHit.collider.gameObject.layer} dist={firstHitDistance:F3}");
                _dbgNextLogTime = Time.unscaledTime + debugLogInterval;
            }
        }

        // 遍历所有格：先做屏幕矩形命中，再将屏幕点投影到该 RectTransform 的世界平面，
        // 计算该点沿射线的距离，与物理命中距离比较以判定遮挡
        foreach (var kv in pos2Block)
        {
            var rt = kv.Value.Rect;
            if (!RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, cam))
                continue;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, mousePos, cam, out var worldPoint))
            {
                float t = Vector3.Dot(worldPoint - ray.origin, ray.direction);
                if (t < 0f) continue; // 在相机后方

                bool notOccluded = t <= firstHitDistance - 1e-4f;
                if (debugRaycast && Time.unscaledTime >= _dbgNextLogTime)
                {
                    Debug.Log($"[LinkLinePuzzle] 候选格 {kv.Key} t={t:F3} vs hit={firstHitDistance:F3} -> {(notOccluded ? "通过" : "被挡")}");
                    _dbgNextLogTime = Time.unscaledTime + debugLogInterval;
                }
                if (notOccluded) return kv.Key;
            }
        }
        return new(-1, -1);
    }

    // 供方块在 Start 注册坐标
    public Vector2Int IndexToGrid(int siblingIndex)
    {
        int row = siblingIndex / cols;
        int col = siblingIndex % cols;
        return new Vector2Int(row, col);
    }

    [ContextMenu("PbL")]
    public void PrintByLine()
    {
        var t = GetAllResults();
        foreach (var t2 in t)
        {
            Debug.Log(t2);
        }
    }

    [ContextMenu("ReserAll")]
    public void ResetAll()
    {
        var ids = from t_line in committedLines
                  select t_line.lineId;
        foreach (var id in ids.ToArray())
        {
            var path = from t_line in committedLines where t_line.lineId == id select t_line.path;
            var pathPos = path.First().ToList();
            var headBlk = pos2Block[pathPos.First()];
            for (int i = 0; i < pathPos.Count; i++)
            {
                pos2Block[pathPos[i]].image.material = LinkLineTextBlock.PublicMat;
            }
            headBlk.ReleaseRuntimeMaterialIfAny(); // Danger for duplicate release
            DeleteCommittedLine(id);
        }
        correctCnt = 0; // 重置答题数
        freezed = false;
    }
    public void FreezedPuzzle()
    {
        freezed = true;
    }
}
