namespace Leap.Data.Utilities {
    internal class Maybe {
        public Maybe(object result) {
            this.WasSuccessful = true;
            this.Result        = result;
        }
        
        private Maybe() { }
        
        public bool WasSuccessful { get; private init; }

        public object Result { get; }

        public static readonly Maybe NotSuccessful = new Maybe { WasSuccessful = false };
    }
}