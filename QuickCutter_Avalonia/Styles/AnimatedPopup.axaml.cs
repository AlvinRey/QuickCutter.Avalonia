using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LibVLCSharp.Shared;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Styles
{
    public class AnimatedPopup : ContentControl
    {
        #region Private Members
        private DispatcherTimer mTimer;
        private TimeSpan framerate = TimeSpan.FromSeconds(1 / 60.0);
        private int mTotalTicks => (int)(_animationTime.TotalSeconds / framerate.TotalSeconds);
        private int mCurrentTick = 0;
        private Size mDesiredSize;
        private Size mDefaultSize;
        private bool mAnimating = false;
        #endregion

        #region Public Properties
        #region Animation Time
        private TimeSpan _animationTime = TimeSpan.FromSeconds(0.17);
        public static readonly DirectProperty<AnimatedPopup, TimeSpan> AnimationTimeProperty = AvaloniaProperty.RegisterDirect<AnimatedPopup, TimeSpan>(
            nameof(AnimationTime), o => o.AnimationTime, (o, v) => o.AnimationTime = v);
        public TimeSpan AnimationTime
        {
            get => (TimeSpan)(_animationTime);
            set => SetAndRaise(AnimationTimeProperty, ref _animationTime, value);
        }
        #endregion
        #endregion


        public AnimatedPopup()
        {
            mTimer = new DispatcherTimer() { Interval = framerate };
            mTimer.Tick += AnimationTick;
            //mTimer.Start();
        }

        private void Reset()
        {
            mCurrentTick = 0;
            Height = mDefaultSize.Height;
            Opacity = 0;
        }
        private void AnimationTick(object? sender, EventArgs e)
        {
            mCurrentTick++;
            if (mCurrentTick > mTotalTicks)
            {
                mTimer.Stop();
                mAnimating = false;
                return;
            }
            var percentageAnimated = (float)mCurrentTick / mTotalTicks;
            var easing = new QuarticEaseIn();

            var finalHeight = mDesiredSize.Height  * easing.Ease(percentageAnimated);
            Opacity = easing.Ease(percentageAnimated);
            Height = finalHeight;
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            if (!mAnimating)
            {
                mDesiredSize = e.NewSize;
            }
            base.OnSizeChanged(e);
        }

        //private Point? GetParentCenter()
        //{

        //    var visualRoot = this.GetVisualRoot() as Control;
        //    if (visualRoot != null)
        //    {
        //        var parentControl = this.GetLogicalParent() as Control;

        //        if (parentControl != null)
        //        {
        //            // 获取父项的中心位置
        //            var bounds = parentControl.Bounds;
        //            var center = bounds.Center;
        //            var transformedCenter = parentControl.TranslatePoint(center, visualRoot);
        //            return transformedCenter;
        //        }
        //    }
        //    return null;
        //}

        public void Stop()
        {
            if (!mAnimating)
            {
                mAnimating = true;
                Reset();
                mTimer.Start();
            }
            else
            {
                mTimer.Stop();
                mAnimating = false;
            }
        }
    }
}
