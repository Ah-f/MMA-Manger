using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class AnimationLoader : MonoBehaviour
{
    #if UNITY_EDITOR
    [MenuItem("MMA/Create Remy Animator Controller")]
    public static void CreateRemyAnimatorController()
    {
        string resourcesPath = "Assets/Resources/Animations";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        string controllerPath = $"{resourcesPath}/RemyAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        var idleState = controller.layers[0].stateMachine.AddState("Idle");
        controller.layers[0].stateMachine.defaultState = idleState;

        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3d/anim/Idle.fbx");
        if (idleClip != null)
        {
            idleState.motion = idleClip;
        }

        var attackStateGroup = controller.layers[0].stateMachine.AddStateMachine("Attacks");
        var hitStateGroup = controller.layers[0].stateMachine.AddStateMachine("Hits");
        var blockStateGroup = controller.layers[0].stateMachine.AddStateMachine("Blocks");
        var moveStateGroup = controller.layers[0].stateMachine.AddStateMachine("Movement");
        var specialStateGroup = controller.layers[0].stateMachine.AddStateMachine("Special");

        RegisterAttacks(controller, attackStateGroup, idleState);
        RegisterHits(controller, hitStateGroup, idleState);
        RegisterBlocks(controller, blockStateGroup, idleState);
        RegisterMovement(controller, moveStateGroup, idleState);
        RegisterSpecial(controller, specialStateGroup, idleState);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("RemyAnimator Controller 생성 완료!");
    }

    private static void RegisterAttacks(AnimatorController controller, AnimatorStateMachine stateGroup, AnimatorState idleState)
    {
        string[] attackAnims = {
            "Jab Cross", "Jab Cross (1)", "Jab Cross (2)",
            "Hook", "Hook (1)", "Hook (2)", "Hook (3)", "Hook (4)",
            "Combo Punch", "Punch Combo",
            "Flying Knee Punch Combo",
            "Mma Kick", "Mma Kick (1)",
            "Drop Kick",
            "Body Jab Cross", "Body Jab Cross (1)", "Body Jab Cross (2)",
            "Double Leg Takedown - Attacker"
        };

        foreach (var animName in attackAnims)
        {
            var state = AddAnimationState(controller, stateGroup, animName);
            if (state != null && idleState != null)
            {
                AddTransition(state, idleState);
            }
        }
    }

    private static void RegisterHits(AnimatorController controller, AnimatorStateMachine stateGroup, AnimatorState idleState)
    {
        string[] hitAnims = {
            "Head Hit", "Head Hit (1)", "Head Hit (2)", "Head Hit (3)", "Head Hit (4)",
            "Light Hit To Head", "Light Hit To Head (1)", "Light Hit To Head (2)",
            "Medium Hit To Head", "Medium Hit To Head (1)", "Medium Hit To Head (2)", "Medium Hit To Head (3)",
            "Big Hit To Head",
            "Hit To Body", "Hit To Body (1)", "Hit To Body (2)", "Hit To Body (3)",
            "Stomach Hit", "Big Stomach Hit",
            "Kidney Hit", "Big Kidney Hit",
            "Rib Hit", "Big Rib Hit",
            "Side Hit", "Big Side Hit",
            "Receiving An Uppercut", "Receiving A Big Uppercut"
        };

        foreach (var animName in hitAnims)
        {
            var state = AddAnimationState(controller, stateGroup, animName);
            if (state != null && idleState != null)
            {
                AddTransition(state, idleState);
            }
        }
    }

    private static void RegisterBlocks(AnimatorController controller, AnimatorStateMachine stateGroup, AnimatorState idleState)
    {
        string[] blockAnims = {
            "Center Block",
            "Left Block", "Left Block (1)",
            "Right Block"
        };

        foreach (var animName in blockAnims)
        {
            var state = AddAnimationState(controller, stateGroup, animName);
            if (state != null && idleState != null)
            {
                AddTransition(state, idleState);
            }
        }
    }

    private static void RegisterMovement(AnimatorController controller, AnimatorStateMachine stateGroup, AnimatorState idleState)
    {
        string[] moveAnims = {
            "Long Step Forward", "Short Step Forward",
            "Step Backward", "Step Backward (1)", "Step Backward (2)",
            "Walking Backwards", "Walking Backwards (1)",
            "Long Left Side Step", "Medium Left Side Step", "Short Left Side Step",
            "Short Right Side Step"
        };

        foreach (var animName in moveAnims)
        {
            var state = AddAnimationState(controller, stateGroup, animName);
            if (state != null && idleState != null)
            {
                AddTransition(state, idleState);
            }
        }
    }

    private static void RegisterSpecial(AnimatorController controller, AnimatorStateMachine stateGroup, AnimatorState idleState)
    {
        string[] specialAnims = {
            "Defeated", "Falling Back Death",
            "Double Leg Takedown - Victim", "Double Leg Takedown - Victim (1)",
            "Capoeira", "Taunt",
            "Illegal Elbow Punch", "Illegal Elbow Punch (1)",
            "Illegal Knee", "Illegal Knee (1)",
            "Illegal Headbutt"
        };

        foreach (var animName in specialAnims)
        {
            var state = AddAnimationState(controller, stateGroup, animName);
            if (state != null && idleState != null)
            {
                AddTransition(state, idleState);
            }
        }
    }

    private static AnimatorState AddAnimationState(AnimatorController controller, AnimatorStateMachine stateGroup, string animName)
    {
        string path = $"Assets/3d/anim/{animName}.fbx";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

        if (clip != null)
        {
            var state = stateGroup.AddState(animName);
            state.motion = clip;
            return state;
        }

        Debug.LogWarning($"애니메이션 클립을 찾을 수 없음: {animName}");
        return null;
    }

    private static void AddTransition(AnimatorState from, AnimatorState to)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0.1f;
    }
    #endif
}
