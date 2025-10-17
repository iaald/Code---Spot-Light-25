using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class DrawingBoardController : MonoBehaviour
{
    [Header("画板设置")]
    public RawImage drawingBoard;
    public ComputeShader drawingComputeShader;
    
    [Header("画笔设置")]
    public Color brushColor = Color.black;
    [Range(1, 50)]
    public int brushSize = 10;
    
    private RenderTexture _renderTexture;
    private int _drawKernel;
    private bool _isDrawing;
    private Vector2 _lastMousePos;
    private Mouse _currentMouse;
    private Vector2 _textureSize;
    
    void Start()
    {
        _currentMouse = Mouse.current;
        InitializeRenderTexture();
        InitializeComputeShader();
    }

    void Update()
    {
        HandleMouseInput();
    }

    private void InitializeRenderTexture()
    {
        // 获取RawImage的尺寸创建RenderTexture
        RectTransform rectTransform = drawingBoard.rectTransform;
        int width = (int)rectTransform.rect.width;
        int height = (int)rectTransform.rect.height;
        
        _textureSize = new Vector2(width, height);
        
        _renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };
        _renderTexture.Create();
        
        // 初始化RenderTexture为白色
        ClearRenderTexture();
        
        drawingBoard.texture = _renderTexture;
        
        Debug.Log($"创建RenderTexture: {width}x{height}");
    }

    private void InitializeComputeShader()
    {
        _drawKernel = drawingComputeShader.FindKernel("CSDraw");
        drawingComputeShader.SetTexture(_drawKernel, "Result", _renderTexture);
        drawingComputeShader.SetVector("brushColor", brushColor);
        drawingComputeShader.SetInt("brushSize", brushSize);
        drawingComputeShader.SetVector("textureSize", _textureSize);
    }

    private void HandleMouseInput()
    {
        if (_currentMouse == null) return;

        Vector2 currentMousePos = _currentMouse.position.ReadValue();
        bool mousePressed = _currentMouse.leftButton.isPressed;

        // 检查鼠标是否在画板区域内
        if (!IsMouseOverDrawingBoard(currentMousePos))
        {
            _isDrawing = false;
            return;
        }

        Vector2 pixelPos = ScreenToPixelPosition(currentMousePos);

        if (mousePressed)
        {
            if (!_isDrawing)
            {
                // 开始新的笔画
                _isDrawing = true;
                _lastMousePos = pixelPos;
                
                // 绘制第一个点
                DrawPoint(pixelPos, pixelPos, true);
            }
            else
            {
                // 继续绘制线条
                if (Vector2.Distance(pixelPos, _lastMousePos) > 1f)
                {
                    DrawPoint(pixelPos, _lastMousePos, false);
                    _lastMousePos = pixelPos;
                }
            }
        }
        else
        {
            _isDrawing = false;
        }
    }

    private void DrawPoint(Vector2 currentPos, Vector2 lastPos, bool isNewStroke)
    {
        drawingComputeShader.SetVector("currentPos", new Vector4(currentPos.x, currentPos.y, 0, 0));
        drawingComputeShader.SetVector("lastPos", new Vector4(lastPos.x, lastPos.y, 0, 0));
        drawingComputeShader.SetBool("isNewStroke", isNewStroke);
        
        // 分派计算着色器 - 覆盖整个纹理
        drawingComputeShader.Dispatch(_drawKernel, 
            Mathf.CeilToInt(_renderTexture.width / 8f), 
            Mathf.CeilToInt(_renderTexture.height / 8f), 1);
    }

    private bool IsMouseOverDrawingBoard(Vector2 screenPos)
    {
        if (drawingBoard.rectTransform == null) return false;
        
        return RectTransformUtility.RectangleContainsScreenPoint(
            drawingBoard.rectTransform, screenPos);
    }

    private Vector2 ScreenToPixelPosition(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingBoard.rectTransform, screenPos, null, out Vector2 localPoint);
        
        Rect rect = drawingBoard.rectTransform.rect;
        
        // 将局部坐标转换为纹理像素坐标
        float x = (localPoint.x - rect.x) / rect.width * _renderTexture.width;
        float y = (localPoint.y - rect.y) / rect.height * _renderTexture.height;
        
        // 确保坐标在有效范围内
        x = Mathf.Clamp(x, 0, _renderTexture.width - 1);
        y = Mathf.Clamp(y, 0, _renderTexture.height - 1);
        
        return new Vector2(x, y);
    }

    public void ClearRenderTexture()
    {
        RenderTexture.active = _renderTexture;
        GL.Clear(true, true, Color.white);
        RenderTexture.active = null;
    }

    [ContextMenu("SetCurrentcolor")]
    private void SetCurrentColor()
    {
        ChangeBrushColor(brushColor);
    }
    public void ChangeBrushColor(Color newColor)
    {
        brushColor = newColor;
        drawingComputeShader.SetVector("brushColor", brushColor);
    }

    [ContextMenu("SetCurrentSize")]
    private void SetCurrentSize()
    {
        ChangeBrushSize(brushSize);
    }
    public void ChangeBrushSize(int newSize)
    {
        brushSize = Mathf.Max(1, newSize);
        drawingComputeShader.SetInt("brushSize", brushSize);
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            DestroyImmediate(_renderTexture);
        }
    }

    // 调试用：在屏幕上显示信息
    private void OnGUI()
    {
        if (_currentMouse != null && drawingBoard != null)
        {
            Vector2 mousePos = _currentMouse.position.ReadValue();
            Vector2 pixelPos = ScreenToPixelPosition(mousePos);
            
            GUI.Label(new Rect(10, 10, 300, 20), $"鼠标位置: {mousePos}");
            GUI.Label(new Rect(10, 30, 300, 20), $"像素位置: {pixelPos}");
            GUI.Label(new Rect(10, 50, 300, 20), $"纹理尺寸: {_textureSize}");
            GUI.Label(new Rect(10, 70, 300, 20), $"绘制中: {_isDrawing}");
        }
    }
}