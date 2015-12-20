﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModel;

namespace WalkingDino
{
    class ClipPlayer
    {
        private Matrix[] boneTransforms;
        private Matrix[] skinTransforms;
        private Matrix[] worldTransforms;

        AnimationClip currentClip;
        List<Keyframe> keyFrameList;

        SkinningData skinData;

        TimeSpan startTime;
        TimeSpan endTime;
        TimeSpan currentTime;

        bool isLooping;

        float fps;

        public ClipPlayer(SkinningData skd, float fps)
        {
            this.skinData = skd;
            this.boneTransforms = new Matrix[skd.BindPose.Count];
            this.skinTransforms = new Matrix[skd.BindPose.Count];
            this.worldTransforms = new Matrix[skd.BindPose.Count];
            this.fps = fps;
        }
        public void play(AnimationClip clip, float startFrame, float endFrame, bool loop)
        {
            this.currentClip = clip;
            // Convert from frame to Xna native Time span
            this.startTime = TimeSpan.FromMilliseconds(startFrame / fps * 1000); // Covert from secounds to milisecounds
            this.endTime = TimeSpan.FromMilliseconds(endFrame / fps * 1000);
            this.currentTime = startTime;

            this.isLooping = loop;

            keyFrameList = currentClip.Keyframes;
        }

        public Matrix[] GetTransformFromTime(TimeSpan ts)
        {
            Matrix[] transform = new Matrix[skinData.BindPose.Count];
            skinData.BindPose.CopyTo(transform, 0);
            int keyNum = 0;

            while(keyNum < keyFrameList.Count)
            {
                Keyframe key = keyFrameList[keyNum];
                if (key.Time > ts)
                    break;
                transform[key.Bone] = key.Transform;
                keyNum++;
            }

            return transform;
        }
        public Matrix[] GetTransformFromTime(float keyFrame)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(keyFrame / fps * 1000);
            return GetTransformFromTime(ts);
        }

        public void Update(TimeSpan time, bool relative, Matrix root)
        {
            if (relative)
                currentTime += time;
            else
                currentTime = time;

            if(currentTime >= endTime)
            {
                if (isLooping)
                    currentTime = startTime;
                else
                    currentTime = endTime;
            }

            // Update bones
            boneTransforms = GetTransformFromTime(currentTime);
            // Root matrix used for moving carachter in the world
            worldTransforms[0] = boneTransforms[0] * root;
            // adjust childerns
            for (int i = 1; i < worldTransforms.Length; i++)
            {
                int parent = skinData.SkeletonHierarchy[i];
                worldTransforms[i] = boneTransforms[i] * worldTransforms[parent]; 
            }

            // Update skin
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = skinData.InverseBindPose[i] * worldTransforms[i];
            }

        }

        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }

    }
}