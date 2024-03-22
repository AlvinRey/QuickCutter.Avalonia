using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.ViewModels
{
    public class SubtitleStreamEditInfoViewModel : ViewModelBase
    {
        private string audio_Or_SubtitleStreamsOptionsKey;
        private SubtitleStreamEditInfo mSubtitleStreamEditInfo;
        private Subject<Unit> mNotifyUpdate;

        public List<SubtitleStreamOriginalInfo> SubtitleStreamOriginalInfoOptions
        {
            get => OutputSettingHandler.SubtitleStreamDictonary[audio_Or_SubtitleStreamsOptionsKey];
        }

        public SubtitleStreamOriginalInfo SubtitleStream
        {
            get => mSubtitleStreamEditInfo.subtitleStream;
            set
            {
                this.RaiseAndSetIfChanged(ref mSubtitleStreamEditInfo.subtitleStream, value, nameof(SubtitleStream));
            }
        }

        public bool IsBurn
        {
            get => mSubtitleStreamEditInfo.isBurn;
            set
            {
                this.RaiseAndSetIfChanged(ref mSubtitleStreamEditInfo.isBurn, value, nameof(IsBurn));
                mNotifyUpdate?.OnNext(Unit.Default);
            }
        }

        public bool CanUserChangeBurnState
        {
            get => mSubtitleStreamEditInfo.canUserChangeBurnState;
            set => this.RaiseAndSetIfChanged(ref mSubtitleStreamEditInfo.canUserChangeBurnState, value, nameof(CanUserChangeBurnState));
        }

        public SubtitleStreamEditInfoViewModel(SubtitleStreamEditInfo subtitleStreamEditInfo, string key, Subject<Unit> subject)
        {
            audio_Or_SubtitleStreamsOptionsKey = key;
            mSubtitleStreamEditInfo = subtitleStreamEditInfo;
            mNotifyUpdate = subject;
        }
    }
}
