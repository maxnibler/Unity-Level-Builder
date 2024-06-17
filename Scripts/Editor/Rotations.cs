namespace LB
{
    public static class Rotations
    {
        public static int EndWall(Ajacents a)
        {
            if (a.Up)
            {
                return 90; 
            }
            if (a.Right)
            {
                return 0; 
            }
            if (a.Down)
            {
                return 270;
            }
            if (a.Left)
            {
                return 180;
            }
            return 0;
        }

        public static int LineWall(Ajacents a)
        {
            if (a.Up)
            {
                return 90;
            }
            return 0;
        }

        public static int CornerWall(Ajacents a)
        {
            if (a.Up)
            {
                if (a.Left)
                {
                    return 180; 
                }
                return 90;
            }
            if (a.Left)
            {
                return 270;
            }
            return 00;
        }

        public static int TWall(Ajacents a)
        {
            if (!a.Left)
            {
                return 0;
            }
            if (!a.Up)
            {
                return 270;
            }
            if (!a.Right)
            {
                return 180;
            }
            if (!a.Down)
            {
                return 90;
            }
            return 0;
        }

        public static int XWall(Ajacents a)
        {
            return 0;
        }
    }
}