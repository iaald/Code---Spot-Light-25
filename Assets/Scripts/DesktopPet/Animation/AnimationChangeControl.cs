using UnityEngine;

public class AnimationChangeControl : MonoBehaviour
{
    private Animator animator;
    private string currentAnimation=("");
    [SerializeField] private int testparameter=0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if(testparameter!=0)
        {
            if (testparameter == 1)
            {
                AnimationChangeTo("Happy1");
            }
            if (testparameter == 2)
            {
                AnimationChangeTo("Angry",0.1f);
            }
            if (testparameter == 3)
            {
                AnimationChangeTo("Drag",0.1f);
            }
            if (testparameter == 4)
            {
                AnimationChangeTo("Worry", 0.1f);
            }
        }
    }
    public void AnimationChangeTo(string animationName,float crossFadeTime=0.15f)
    {
        if(currentAnimation != animationName) 
        {
            currentAnimation = animationName;
            animator.CrossFade(animationName, crossFadeTime);
        }
    }
    // Update is called once per frame
    
}
