#region

using System;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace FileBackupper {
    internal class ProgressBar: IDisposable {
        private const int BlockCount = 10;
        private const string Animation = @"|/-\";
        private const char BlockChar = '■';
        private const char EmptyChar = '-';
        private const int AnimationInterval = 100;

        private readonly Timer timer;
        private int animationIndex;
        private char currentAnimationChar = Animation.First();
        private string currentMessage = string.Empty;
        private string currentOutput = string.Empty;
        private double currentProgress;

        public ProgressBar() {
            this.timer = new Timer(TimerHandler, new object(), AnimationInterval, AnimationInterval);
            this.animationIndex = 0;
            this.currentProgress = 0.0;
            this.IsDisposed = false;
        }

        public bool IsDisposed {
            get;
            private set;
        }

        public void Dispose() {
            lock (this.timer) {
                this.IsDisposed = true;
                //ClearText();
                Console.WriteLine();
                timer.Dispose();
            }
        }

        private void TimerHandler(object state) {
            lock (this.timer) {
                if (this.IsDisposed) {
                    return;
                }

                this.currentAnimationChar = Animation[this.animationIndex++ % Animation.Length];
                this.UpdateText();
            }
        }

        internal void UpdateProgress(double progress, string message) {
            this.currentProgress = progress;
            if (this.currentProgress > 0.999) {
                this.currentAnimationChar = '-';
            }
            this.currentMessage = message;
            this.UpdateText();
        }

        private void ClearText() {
            lock (this.currentOutput) {
                var outputBuilder = new StringBuilder();
                outputBuilder.Append('\b', this.currentOutput.Length);
                outputBuilder.Append(' ', this.currentOutput.Length);
                outputBuilder.Append('\b', this.currentOutput.Length);
                Console.Write(outputBuilder);
                this.currentOutput = string.Empty;
            }
        }

        private void UpdateText() {
            lock (this.currentOutput) {
                lock (this.currentMessage) {
                    var percent = (int) (this.currentProgress * 100);
                    var progressBlockCount = (int) (this.currentProgress * BlockCount);
                    var newText =
                        $"[{new string(BlockChar, progressBlockCount)}{new string(EmptyChar, BlockCount - progressBlockCount)}] {percent,3}% {currentAnimationChar} {currentMessage}";
                    newText = newText[..Math.Min(newText.Length, Console.BufferWidth - 1)];

                    // Get length of common portion
                    var commonPrefixLength = 0;
                    var commonLength = Math.Min(this.currentOutput.Length, newText.Length);
                    while (commonPrefixLength < commonLength &&
                           newText[commonPrefixLength] == this.currentOutput[commonPrefixLength]) {
                        commonPrefixLength++;
                    }

                    // Backtrack to the first differing character
                    var outputBuilder = new StringBuilder();
                    outputBuilder.Append('\b', this.currentOutput.Length - commonPrefixLength);

                    // Output new suffix
                    outputBuilder.Append(newText[commonPrefixLength..]);

                    // If the new text is shorter than the old one: delete overlapping characters
                    var overlapCount = this.currentOutput.Length - newText.Length;
                    if (overlapCount > 0) {
                        outputBuilder.Append(' ', overlapCount);
                        outputBuilder.Append('\b', overlapCount);
                    }

                    Console.Write(outputBuilder);
                    this.currentOutput = newText;
                }
            }
        }
    }
}