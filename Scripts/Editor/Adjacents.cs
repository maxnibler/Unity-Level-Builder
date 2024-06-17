namespace LB 
{
        public struct Ajacents
        {
            public bool Up;
            public bool Right;
            public bool Down;
            public bool Left;
            public int Count 
            {
                get 
                {
                    return 0 + b(Up) + b(Down) + b(Right) + b(Left);
                }
            }

            private int b(bool dir)
            {
                return dir ? 1:0;
            }

            public Ajacents(int garbage)
            {
                Up = false;
                Right = false;
                Down = false;
                Left = false;
            }

            public override string ToString()
            {
                return string.Format("Up: {0}, Down: {1}, Left: {2}, Right: {3}", Up, Down, Left, Right);
            }

            public void Reset()
            {
                Up = false;
                Right = false;
                Down = false;
                Left = false;
            }
        }
}