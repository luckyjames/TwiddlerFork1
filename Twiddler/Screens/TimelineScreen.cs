﻿using System;
using Caliburn.PresentationFramework.Screens;
using Twiddler.Screens.Interfaces;

namespace Twiddler.Screens
{
    public class TimelineScreen : ScreenConductor<ITweetScreen>.WithCollection.AllScreensActive
    {
        public TimelineScreen() : base(false)
        {
        }
    }

    public interface ITweet
    {
    }

    public interface ITimeline
    {
        IObservable<ITweet> Tweets { get; }
    }
}