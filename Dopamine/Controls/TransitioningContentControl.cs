using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Dopamine.Controls
{
    /// <summary>
    /// A ContentControl that smoothly animates its content when it changes.
    /// Uses GPU-composited RenderTransform (TranslateTransform) — never Margin.
    /// Easing: Material Design M3 "Emphasized Decelerate" (0.05, 0.7, 0.1, 1.0).
    ///
    /// Rapid navigation bug fix: each Animate() call increments _animVersion.
    /// Completed lambdas capture the version at launch time and only apply
    /// cleanup side-effects if no newer animation has since started, so a stale
    /// Completed handler can never kill a live animation.
    /// </summary>
    public class TransitioningContentControl : ContentControl
    {
        public static readonly DependencyProperty FadeInProperty =
            DependencyProperty.Register("FadeIn", typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(false));
        public static readonly DependencyProperty FadeInTimeoutProperty =
            DependencyProperty.Register("FadeInTimeout", typeof(double), typeof(TransitioningContentControl), new PropertyMetadata(0.5));
        public static readonly DependencyProperty SlideInProperty =
            DependencyProperty.Register("SlideIn", typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(false));
        public static readonly DependencyProperty SlideInTimeoutProperty =
            DependencyProperty.Register("SlideInTimeout", typeof(double), typeof(TransitioningContentControl), new PropertyMetadata(0.5));
        public static readonly DependencyProperty SlideInFromProperty =
            DependencyProperty.Register("SlideInFrom", typeof(int), typeof(TransitioningContentControl), new PropertyMetadata(0));
        public static readonly DependencyProperty SlideInToProperty =
            DependencyProperty.Register("SlideInTo", typeof(int), typeof(TransitioningContentControl), new PropertyMetadata(0));
        public static readonly DependencyProperty RightToLeftProperty =
            DependencyProperty.Register("RightToLeft", typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(false));

        // Monotonically-increasing version. Each Animate() call captures the current
        // value — Completed handlers only act if they still own the latest version.
        private int _animVersion = 0;

        public bool FadeIn
        {
            get { return (bool)GetValue(FadeInProperty); }
            set { SetValue(FadeInProperty, value); }
        }
        public double FadeInTimeout
        {
            get { return (double)GetValue(FadeInTimeoutProperty); }
            set { SetValue(FadeInTimeoutProperty, value); }
        }
        public bool SlideIn
        {
            get { return (bool)GetValue(SlideInProperty); }
            set { SetValue(SlideInProperty, value); }
        }
        public double SlideInTimeout
        {
            get { return (double)GetValue(SlideInTimeoutProperty); }
            set { SetValue(SlideInTimeoutProperty, value); }
        }
        public int SlideInFrom
        {
            get { return (int)GetValue(SlideInFromProperty); }
            set { SetValue(SlideInFromProperty, value); }
        }
        public int SlideInTo
        {
            get { return (int)GetValue(SlideInToProperty); }
            set { SetValue(SlideInToProperty, value); }
        }
        public bool RightToLeft
        {
            get { return (bool)GetValue(RightToLeftProperty); }
            set { SetValue(RightToLeftProperty, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            this.Animate();
        }

        private void Animate()
        {
            bool doFade  = this.FadeIn  && this.FadeInTimeout  > 0;
            bool doSlide = this.SlideIn && this.SlideInTimeout > 0 && this.SlideInFrom != 0;

            if (!doFade && !doSlide) return;

            // Bump version — any Completed handlers from previous animations will
            // see a stale version and skip their cleanup, leaving this animation alone.
            int version = ++_animVersion;

            // Material Design M3 "Emphasized Decelerate": shoots in fast, decelerates smoothly.
            var easing = new KeySpline(0.05, 0.7, 0.1, 1.0);

            if (doFade)
            {
                var anim = new DoubleAnimationUsingKeyFrames
                {
                    Duration     = new Duration(TimeSpan.FromSeconds(this.FadeInTimeout)),
                    FillBehavior = FillBehavior.Stop,
                };
                anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromPercent(0)));
                anim.KeyFrames.Add(new SplineDoubleKeyFrame(1.0, KeyTime.FromPercent(1), easing));
                anim.Completed += (_, __) =>
                {
                    // Only commit final state if we are still the active animation.
                    if (_animVersion == version)
                        this.Opacity = 1.0;
                };
                this.BeginAnimation(OpacityProperty, anim);
            }

            if (doSlide)
            {
                double fromX = this.RightToLeft ? -this.SlideInFrom : this.SlideInFrom;

                // Fresh transform every time — previous animated transform is discarded.
                var translate = new TranslateTransform(fromX, 0);
                this.RenderTransform = translate;

                var anim = new DoubleAnimationUsingKeyFrames
                {
                    Duration     = new Duration(TimeSpan.FromSeconds(this.SlideInTimeout)),
                    FillBehavior = FillBehavior.Stop,
                };
                anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(fromX, KeyTime.FromPercent(0)));
                anim.KeyFrames.Add(new SplineDoubleKeyFrame(0.0, KeyTime.FromPercent(1), easing));
                anim.Completed += (_, __) =>
                {
                    // Only clean up if we are still the active animation.
                    if (_animVersion == version)
                        this.RenderTransform = Transform.Identity;
                };
                translate.BeginAnimation(TranslateTransform.XProperty, anim);
            }
        }
    }
}
