using QuickCutter_Avalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace QuickCutter_Avalonia.ViewModels
{
    public class SelecteAudioStreamViewModel : ViewModelBase
    {
        private bool mIsSelected;
        private AudioStreamOriginalInfo mHostedAudioStream;
        private List<AudioStreamOriginalInfo> mSelectedCollection;


        public string StreamName
        {
            get => mHostedAudioStream.name;
        }

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref mIsSelected, value, nameof(IsSelected));
                if (value)
                {
                    mSelectedCollection.Add(mHostedAudioStream);
                }
                else
                {
                    mSelectedCollection.Remove(mHostedAudioStream);
                }
            }
        }

        public SelecteAudioStreamViewModel(AudioStreamOriginalInfo hostedStream, List<AudioStreamOriginalInfo> selectedCollection)
        {
            mHostedAudioStream = hostedStream;
            mSelectedCollection = selectedCollection;

            if (mSelectedCollection.Contains(mHostedAudioStream))
                mIsSelected = true;
        }
    }

    public class SelecteSubtitleStreamViewModel : ViewModelBase
    {
        private bool mIsSelected;
        private bool mCanSelect = true;
        private SubtitleStreamOriginalInfo mHostedSubtitleStream;
        private List<SubtitleStreamOriginalInfo> mSelectedCollection;
        private event Action OnSelectedChanged;

        public string StreamName
        {
            get => mHostedSubtitleStream.name;
        }

        public bool IsTextType
        {
            get => mHostedSubtitleStream.isTextType;
        }

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref mIsSelected, value, nameof(IsSelected));
                if(value)
                {
                    mSelectedCollection.Add(mHostedSubtitleStream);
                }
                else
                {
                    mSelectedCollection.Remove(mHostedSubtitleStream);
                }
                OnSelectedChanged?.Invoke();
            }
        }

        public bool CanSelect
        {
            get => mCanSelect;
            set
            {
                if(!value && IsSelected) { IsSelected = false; }
                this.RaiseAndSetIfChanged(ref mCanSelect, value, nameof(CanSelect));
            }
        }

        public SelecteSubtitleStreamViewModel(SubtitleStreamOriginalInfo hostedStream, List<SubtitleStreamOriginalInfo>selectedCollection, Action onSelectedChanged)
        {
            mHostedSubtitleStream = hostedStream;
            mSelectedCollection = selectedCollection;

            if(mSelectedCollection.Contains(mHostedSubtitleStream)) 
                mIsSelected = true;

            OnSelectedChanged += onSelectedChanged;
        }

        public void UpdateCanSelectState(bool isBurn, bool? isSelectTextType, SelecteSubtitleStreamViewModel? firstSelectedSubtitle = null)
        {
            if(IsTextType && isBurn && isSelectTextType.HasValue && isSelectTextType.Value) // TextType, burn, already SelectTextType
            {
                if(firstSelectedSubtitle != this)
                {
                    CanSelect = false;
                }
                else
                {
                    CanSelect = true;
                }
            }
            else if (IsTextType && !isBurn && isSelectTextType.HasValue && isSelectTextType.Value)// TextType, not burn, already SelectTextType
            {
                CanSelect = true;
            }
            else if (IsTextType && isBurn && isSelectTextType.HasValue && !isSelectTextType.Value)// TextType, burn, already SelectImageType
            {
                CanSelect = false;
            }
            else if (IsTextType && !isBurn && isSelectTextType.HasValue && !isSelectTextType.Value)// TextType, not burn, already SelectImageType
            {
                CanSelect = true;
            }
            else if (!IsTextType && isBurn && isSelectTextType.HasValue && isSelectTextType.Value)// Image, burn, already SelectTextType
            {
                CanSelect = false;
            }
            else if (!IsTextType && !isBurn && isSelectTextType.HasValue && isSelectTextType.Value)// Image, not burn, already SelectTextType
            {
                CanSelect = false;
            }
            else if (!IsTextType && isBurn && isSelectTextType.HasValue && !isSelectTextType.Value)// Image, burn, already SelectImageType
            {
                if (firstSelectedSubtitle != this)
                {
                    CanSelect = false;
                }
                else
                {
                    CanSelect = true;
                }
            }
            else if (!IsTextType && !isBurn && isSelectTextType.HasValue && !isSelectTextType.Value)// Image, not burn, already SelectImageType
            {
                CanSelect = false;
            }
            else if (IsTextType && isBurn && !isSelectTextType.HasValue)// TextType, burn, not select any one
            {
                CanSelect = true;
            }
            else if (IsTextType && !isBurn && !isSelectTextType.HasValue)// TextType, not burn, not select any one
            {
                CanSelect = true;
            }
            else if (!IsTextType && isBurn && !isSelectTextType.HasValue)// Image, burn, not select any one
            {
                CanSelect = true;
            }
            else if (!IsTextType && !isBurn && !isSelectTextType.HasValue)// Image, not burn, not select any one
            {
                CanSelect = false;
            }
        }
    }
}
