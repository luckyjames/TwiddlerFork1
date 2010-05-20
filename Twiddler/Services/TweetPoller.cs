using System;
using System.Linq;
using Caliburn.Core.IoC;
using TweetSharp.Extensions;
using TweetSharp.Twitter.Extensions;
using TweetSharp.Twitter.Fluent;
using TweetSharp.Twitter.Model;
using Twiddler.Models.Interfaces;
using Twiddler.Services.Interfaces;

namespace Twiddler.Services
{
    [PerRequest(typeof (ITweetPoller))]
    public class TweetPoller : ITweetPoller
    {
        private readonly Func<TwitterStatus, ITweet> _tweetFactory;

        private IFluentTwitter _twitter;

        public TweetPoller(Func<TwitterStatus, ITweet> tweetFactory)
        {
            _tweetFactory = tweetFactory;
        }

        #region ITweetPoller Members

        public event EventHandler<NewTweetsEventArgs> NewTweets = delegate { };

        public void Start()
        {
            _twitter =
                FluentTwitter.
                    CreateRequest().
                    Statuses().
                    OnHomeTimeline().
                    Configuration.
                    UseRateLimiting(20.Percent()).
                    RepeatEvery(25.Seconds());

            _twitter.CallbackTo(GotTweets);

            _twitter.BeginRequest();
        }

        public void Stop()
        {
            _twitter.Cancel();
        }

        /// <summary>
        /// Releases all resources used by an instance of the <see cref="TweetPoller" /> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)" /> method, passing in <strong>true</strong>, and then suppresses 
        /// finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="TweetPoller" /> class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual <see cref="Dispose(bool)" /> method, passing in <strong>false</strong>.
        /// </remarks>
        ~TweetPoller()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="TweetPoller" /> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Stop();
        }

        private void GotTweets(object sender, TwitterResult result, object userstate)
        {
            if (!result.SkippedDueToRateLimiting)
                NewTweets(this, new NewTweetsEventArgs(result.
                                                           AsStatuses().
                                                           Select(x => _tweetFactory(x))));
        }
    }
}