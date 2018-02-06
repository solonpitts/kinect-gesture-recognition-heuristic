using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
/**
*  Written by Solon Pitts for CS6331.001, Semester Project, Fall 2017.
*  NetID: sxp146230
*  GestureRecognizer.cs: Tracks various gestures using HNSkeleton data
* */
public class GestureRecognizer : HNStreamDataReceiver
{
    public HNSkeleton _skeleton;
    protected Vector3[] joints;
    //keeps track of the joints in the last Frame, useful for finding frame to frame displacement
    private Vector3[] lastFrameJoints;
    public Text text;

    //Tracks the frame number, useful for keeping track of the duration of gestures
    private int frameNumber = 0;
    //if true, then the program loops back to the beginning of the file
    public bool loop = true;
    
    //Determine if the gesture should be tracked
    public bool Detect_Wave_Right, Detect_Wave_Left, Detect_Pump_Right, Detect_Pump_Left, Detect_Step_Left, 
        Detect_Step_Right, Detect_Kick_Left, Detect_Kick_Right, Detect_Sitting, Detect_Shrug, Detect_Bowing;
    public HNHANDSTATE leftHand, rightHand;
    //The actual state of the gesture
    public bool wave_right, wave_left, shrug, fistpump_left, fistpump_right, sitting, left_step, right_step,
        kick_right, kick_left, bowing = false;
    
    //Hand frame to frame horizontal displacement
    private float leftHandHorizDisplacement;
    private int leftHandHorizDelay;//the delay before we care that it stopped moving
    private float rightHandHorizDisplacement;
    private int rightHandHorizDelay;

    //Shoulder Vertical Displacement
    private float rightShoulderVertDisplacement;
    private int rightShoulderVertDelay;
    private float leftShoulderVertDisplacement;
    private int leftShoulderVertDelay;

    //Hand Vertical Displacement
    private float rightHandVertDisplacement;
    private int rightHandVertDelay;
    private float leftHandVertDisplacement;
    private int leftHandVertDelay;

    //at a max, wait 50 frames before caring that it stopped moving
    private static int MAX_HORIZONTAL_DELAY = 50;
    private static float HORIZ_DISPLACEMENT_THRESHOLD = .2f;
    private static int MAX_VERTICAL_DELAY = 50;
    private static int MAX_VERTICAL_LARGE_DELAY = 100;
    private static float VERT_SMALL_DISPLACEMENT_THRESHOLD = .05f;
    private static float VERT_LARGE_DISPLACEMENT_THRESHOLD = .2f;

    //For Initialization
    void Start()
    {
        //text = GetComponent<Text>();
        joints = new Vector3[0];
        lastFrameJoints = new Vector3[0];
        leftHand = _skeleton.leftHand;
        rightHand = _skeleton.rightHand;

        leftHandHorizDisplacement = 0f;
        rightHandHorizDisplacement = 0f;
        rightHandHorizDelay = 0;
        leftHandHorizDelay = 0;

        leftShoulderVertDisplacement = 0f;
        rightShoulderVertDisplacement = 0f;
        rightShoulderVertDelay = 0;
        leftShoulderVertDelay = 0;

        leftHandVertDisplacement = 0f;
        rightHandVertDisplacement = 0f;
        rightHandVertDelay = 0;
        leftHandVertDelay = 0;

        Detect_Wave_Right = true;
        Detect_Wave_Left = true;
        Detect_Pump_Right = true;
        Detect_Pump_Left = true;
        Detect_Step_Left = true;
        Detect_Step_Right = true;
        Detect_Kick_Left = true;
        Detect_Kick_Right = true;
        Detect_Sitting = true;
        Detect_Shrug = true;
        Detect_Bowing =true;
    }

    // Update is called once per frame, calls detect methods
    void FixedUpdate()
    {
        lock (this)
        {
            if (_skeleton.getTimestamp() > _currentTimestamp || loop)
            {
                frameNumber++;
                if (frameNumber > 60)
                    frameNumber = 0;

                updateSkeleton();
                _currentTimestamp = _skeleton.getTimestamp();
                if (joints.Length > 0)
                {
                    if (Detect_Sitting)
                        sitting = detectSitting();
                    if (Detect_Step_Left)
                        left_step = detectStep(HNJOINT.KNEE_LEFT);
                    if (Detect_Step_Right)
                        right_step = detectStep(HNJOINT.KNEE_RIGHT);
                    if (Detect_Kick_Left)
                        kick_left = detectKick(HNJOINT.FOOT_LEFT);
                    if (Detect_Kick_Right)
                        kick_right = detectKick(HNJOINT.FOOT_RIGHT);
                    if (Detect_Bowing)
                        bowing = detectBowing();
                    if (Detect_Shrug)
                        shrug = detectShrug();
                    if (Detect_Wave_Right)
                        wave_right = detectWave(HNJOINT.HAND_RIGHT);
                    if (Detect_Wave_Left)
                        wave_left = detectWave(HNJOINT.HAND_LEFT);
                    if(Detect_Pump_Right)
                        fistpump_right = detectFistPump(HNJOINT.HAND_RIGHT) && !wave_right;
                    if(Detect_Pump_Left)
                        fistpump_left = detectFistPump(HNJOINT.HAND_LEFT) && !wave_left;
                    if(text!=null)
                        updateText();
                }
            }
        }
    }

    /**
    * Updates the text displayed in the UI text component
    * */
    private void updateText()
    {
        String str = "Skeleton Gestures Tracked:\n\n";
        if (Detect_Sitting)
        {
            str += "Sitting = ";
            if (sitting)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Bowing)
        {
            str += "Bowing = ";
            if (bowing)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Step_Right)
        {
            str += "Step Right = ";
            if (right_step)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Step_Left)
        {
            str += "Step Left = ";
            if (left_step)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Kick_Right)
        {
            str += "Kick Right = ";
            if (kick_right)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Kick_Left)
        {
            str += "Kick Left = ";
            if (kick_left)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Wave_Right)
        {
            str += "Wave Right = ";
            if (wave_right)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Wave_Left)
        {
            str += "Wave Left = ";
            if (wave_left)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Pump_Left)
        {
            str += "Fistpump Left = ";
            if (fistpump_left)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Pump_Right)
        {
            str += "Fistpump Right = ";
            if (fistpump_right)
                str += "true\n";
            else
                str += "false\n";
        }
        if (Detect_Shrug)
        {
            str += "Shrug = ";
            if (shrug)
                str += "true\n";
            else
                str += "false\n";
        }
        text.text = str;
    }

    /**
    * Updates the Skeleton joints
    * */
    protected virtual void updateSkeleton()
    {
        if(joints.Length>0 && (frameNumber==60))
        {
            lastFrameJoints = joints;
        }
        joints = _skeleton.getJoints();
        leftHand = _skeleton.leftHand;
        rightHand = _skeleton.rightHand;
    }

    /**
    * Detects if the skeleton is stepping
    * */
    private bool detectStep(HNJOINT jointype)
    {
        var knee = joints[(int)jointype];
        var hip = joints[(int)HNJOINT.HIP_CENTER];
        Vector3 foot;
        if (jointype == HNJOINT.KNEE_LEFT)
            foot = joints[(int)HNJOINT.FOOT_LEFT];
        else
            foot = joints[(int)HNJOINT.FOOT_RIGHT];
        return (Math.Max(hip.y - knee.y, knee.y - hip.y) < .2f) && !sitting && (Math.Max(knee.x - foot.x, foot.x - knee.x) < .1f);
    }

    /**
    * Detects if the skeleton is sitting
    * */
    private bool detectSitting()
    {
        var hip = joints[(int)HNJOINT.HIP_CENTER];
        var leftKnee = joints[(int)HNJOINT.KNEE_LEFT];
        var rightKnee = joints[(int)HNJOINT.KNEE_RIGHT];
        //are the legs close in height
        if (Math.Max(hip.y - leftKnee.y, leftKnee.y - hip.y) < .1f && Math.Max(hip.y - rightKnee.y, rightKnee.y - hip.y) < .1f)
            return true;
        return false;
    }

    
    /**
    * Detects if the given hand is doing the fistpump gesture
    * */
    private bool detectFistPump(HNJOINT handType)
    {
        getVerticalDisplacement(handType);
        bool ishandMovingVertical;
        if (handType == HNJOINT.HAND_LEFT)
            ishandMovingVertical = (leftHandVertDelay > 0); //if moved at VERT_LARGE_DISPLACEMENT_THRESHOLD in the last MAX_VERTICAL_DELAY frames
        else
            ishandMovingVertical = (rightHandVertDelay > 0);

        return !shrug && detectHandHorizontal(handType, .1f) && isHandClosed(handType) && isHandOverElbow(handType) && ishandMovingVertical;
    }
 
    /**
    * Detects if the body is shrugging
    * */
    private bool detectShrug()
    {
        getVerticalDisplacement(HNJOINT.SHOULDER_LEFT);
        getVerticalDisplacement(HNJOINT.SHOULDER_RIGHT);
        bool areShouldersMoving = leftShoulderVertDelay > 0 && rightShoulderVertDelay > 0;

        return isHandOpen(HNJOINT.HAND_LEFT) && isHandOpen(HNJOINT.HAND_RIGHT) && isHandOverElbow(HNJOINT.HAND_RIGHT) && isHandOverElbow(HNJOINT.HAND_LEFT) && areShouldersMoving;
    }

    /**
    * Detects if the given hand is waving
    * */
    private bool detectWave(HNJOINT handType)
    {
        bool isHandOpened = isHandOpen(handType);
        bool handOverElbow = isHandOverElbow(handType);
        bool isHandWithinHorizontalBounds = detectHandHorizontal(handType, .4f);
        bool isMovingHorizontal;
        getHorizontalDisplacement(handType);

        if (handType == HNJOINT.HAND_LEFT)
            isMovingHorizontal = (leftHandHorizDelay > 0); //if moved at that speed in the last MAX_HORIZONTAL_DELAY frames
        else
            isMovingHorizontal = (rightHandHorizDelay > 0);
        return !shrug && handOverElbow && isHandWithinHorizontalBounds && isMovingHorizontal && isHandOpened;
    }

    /**
    * Detects if the skeleton is bowing
    * */
    private bool detectBowing()
    {
        var hip = joints[(int)HNJOINT.HIP_CENTER];
        var shoulder = joints[(int)HNJOINT.SHOULDER_CENTER];
        return (shoulder.y - hip.y < .6f);
    }

    /**
    * Detects if the skeleton is kicking
    * */
    private bool detectKick(HNJOINT jointype)
    {
        var foot = joints[(int)jointype];
        var hip = joints[(int)HNJOINT.HIP_CENTER];
        return (Math.Max(hip.y - foot.y, foot.y - hip.y) < .5f && Math.Max(hip.x - foot.x, foot.x - hip.x) > .3f);
    }
   
    /**
    * Detects if the given hand is open
    * */
    private bool isHandOpen(HNJOINT handType)
    {
        if (handType == HNJOINT.HAND_LEFT)
            return leftHand != HNHANDSTATE.HNHANDSTATE_CLOSED;
        else
            return rightHand != HNHANDSTATE.HNHANDSTATE_CLOSED;
    }

    /**
    * Detects if the given hand is closed
    * */
    private bool isHandClosed(HNJOINT handType)
    {
        if (handType == HNJOINT.HAND_LEFT)
            return leftHand != HNHANDSTATE.HNHANDSTATE_OPEN;
        else
            return rightHand != HNHANDSTATE.HNHANDSTATE_OPEN;
    }

    /**
    * Detects if any Joint is moving horizontally, and returns the distance
    * */
    private float getHorizontalDisplacement(HNJOINT jointType)
    {
        var joint = joints[(int)jointType];
        Vector3 lastJoint;

        if (lastFrameJoints.Length > 0)
        {
            lastJoint = lastFrameJoints[(int)jointType];
            if(jointType == HNJOINT.HAND_RIGHT)
            {
                if (rightHandHorizDelay > 0)//if the delay is > 0, that means we've detected horiz movement
                    rightHandHorizDelay++;
                if (rightHandHorizDelay > MAX_HORIZONTAL_DELAY)
                {
                    rightHandHorizDelay = 0;
                }
                float displacement = rightHandHorizDisplacement;
                if(Math.Max(joint.x - lastJoint.x, lastJoint.x - joint.x) < 1f)
                    displacement = Math.Max(joint.x - lastJoint.x, lastJoint.x - joint.x);
                //if we've got displacement, then start the delay at 1
                if (displacement > HORIZ_DISPLACEMENT_THRESHOLD)
                {
                    rightHandHorizDelay = 1;
                    rightHandHorizDisplacement = displacement;
                }

                return displacement;
            }
            else if (jointType == HNJOINT.HAND_LEFT)
            {
                if (leftHandHorizDelay > 0)
                    leftHandHorizDelay++;
                if (leftHandHorizDelay > MAX_HORIZONTAL_DELAY)
                {
                    leftHandHorizDelay = 0;
                }
                float displacement = leftHandHorizDisplacement;
                if (Math.Max(joint.x - lastJoint.x, lastJoint.x - joint.x) < 1f)
                    displacement = Math.Max(joint.x - lastJoint.x, lastJoint.x - joint.x);
                //if we've got displacement, then start the delay at 1
                if (displacement > HORIZ_DISPLACEMENT_THRESHOLD)
                {
                    leftHandHorizDelay = 1;
                    leftHandHorizDisplacement = displacement;
                }

                return displacement;
            }
                
            return Math.Max(joint.x - lastJoint.x, lastJoint.x - joint.x);
        }
        else
            return 0;
    }

    /**
    * Detects if any Joint is moving vertically, and returns the distance
    * */
    private float getVerticalDisplacement(HNJOINT jointType)
    {
        var joint = joints[(int)jointType];
        Vector3 lastJoint;

        if (lastFrameJoints.Length > 0)
        {
            lastJoint = lastFrameJoints[(int)jointType];
            if (jointType == HNJOINT.SHOULDER_RIGHT)
            {
                if (rightShoulderVertDelay > 0)//if the delay is > 0, that means we've detected horiz movement
                    rightShoulderVertDelay++;
                if (rightShoulderVertDelay > MAX_VERTICAL_DELAY)
                {
                    rightShoulderVertDelay = 0;
                }
                float displacement = rightShoulderVertDisplacement;
                if (Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y) < 1f)
                    displacement = Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y);
                //if we've got displacement, then start the delay at 1
                if (displacement > VERT_SMALL_DISPLACEMENT_THRESHOLD)
                {
                    rightShoulderVertDelay = 1;
                    rightShoulderVertDisplacement = displacement;
                }

                return displacement;
            }
            else if (jointType == HNJOINT.SHOULDER_LEFT)
            {
                if (leftShoulderVertDelay > 0)
                    leftShoulderVertDelay++;
                if (leftShoulderVertDelay > MAX_VERTICAL_DELAY)
                {
                    leftShoulderVertDelay = 0;
                }
                float displacement = leftShoulderVertDisplacement;
                if (Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y) < 1f)
                    displacement = Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y);
                //if we've got displacement, then start the delay at 1
                if (displacement > VERT_SMALL_DISPLACEMENT_THRESHOLD)
                {
                    leftShoulderVertDelay = 1;
                    leftShoulderVertDisplacement = displacement;
                }

                return displacement;
            }
            else if (jointType == HNJOINT.HAND_RIGHT)
            {
                if (rightHandVertDelay > 0)//if the delay is > 0, that means we've detected horiz movement
                    rightHandVertDelay++;
                if (rightHandVertDelay > MAX_VERTICAL_LARGE_DELAY)
                {
                    rightHandVertDelay = 0;
                }
                float displacement = rightHandVertDisplacement;
                if (Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y) < 1f)
                    displacement = Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y);
                //if we've got displacement, then start the delay at 1
                if (displacement > VERT_LARGE_DISPLACEMENT_THRESHOLD)
                {
                    rightHandVertDelay = 1;
                    rightHandVertDisplacement = displacement;
                }

                return displacement;
            }
            else if (jointType == HNJOINT.HAND_LEFT)
            {
                if (leftHandVertDelay > 0)
                    leftHandVertDelay++;
                if (leftHandVertDelay > MAX_VERTICAL_LARGE_DELAY)
                {
                    leftHandVertDelay = 0;
                }
                float displacement = leftHandVertDisplacement;
                if (Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y) < 1f)
                    displacement = Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y);
                //if we've got displacement, then start the delay at 1
                if (displacement > VERT_LARGE_DISPLACEMENT_THRESHOLD)
                {
                    leftHandVertDelay = 1;
                    leftHandVertDisplacement = displacement;
                }

                return displacement;
            }

            return Math.Max(joint.y - lastJoint.y, lastJoint.y - joint.y);
        }
        else
            return 0;
    }

    /**
    * Detects if the hand is too far from the x pos of the elbow
    * */
    private bool detectHandHorizontal(HNJOINT handType, float maxHorizontalDistance)
    {
        //float maxHorizontalDistance = .25f;
        HNJOINT elbowType;
        if (handType == HNJOINT.HAND_RIGHT)
            elbowType = HNJOINT.ELBOW_RIGHT;
        else
            elbowType = HNJOINT.ELBOW_LEFT;
        var hand = joints[(int)handType];
        var elbow = joints[(int)elbowType];

        var horizontalDifference = hand.x - elbow.x;

        return (horizontalDifference < maxHorizontalDistance) && (horizontalDifference > 0 - maxHorizontalDistance);
    }

    /**
    * Detects if the hand is above the y of the elbow
    * */
    private bool isHandOverElbow(HNJOINT handType)
    {
        HNJOINT elbowType;
        if (handType == HNJOINT.HAND_RIGHT)
            elbowType = HNJOINT.ELBOW_RIGHT;
        else
            elbowType = HNJOINT.ELBOW_LEFT;
        var hand = joints[(int)handType];
        var elbow = joints[(int)elbowType];
        return hand.y > elbow.y;
    }

    /**
    * Updates the HNSkeleton
    * */
    public override void updateData(HNStreamingData data)
    {
        lock (this)
        {
            _skeleton = (HNSkeleton)data;
        }
    }
}
