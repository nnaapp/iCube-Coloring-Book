using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace BookLib
{
    public static class BookLib
    {
        private const ulong GREEN_CONST = 1000; // 10^3 | Multiply green values by this to add zeroes, for 1,000 255 becomes 255000
        private const ulong BLUE_CONST = 1000000;// 10^6 | Same idea here, for 1,000,000 255 becomes 255000000
        private const ulong ALPHA_CONST = 1000000000; // 10^9 | Same idea

        public struct Int2 // int vector2
        {
            public Int2(Vector2 v)
            {
                x = (int)v.x;
                y = (int)v.y;
            }

            public Int2(int X, int Y)
            {
                x = X;
                y = Y;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Int2))
                    return false;

                Int2 i2 = (Int2)obj;
                return this.x == i2.x && this.y == i2.y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return x + (y * 10);
                }
            }

            public int x;
            public int y;
        }

        public struct IntBytePair
        {
            public IntBytePair(int i, byte b)
            {
                Int = i;
                Byte = b;
            }

            public int Int;
            public byte Byte;
        }

        public class Color32Comparer : IEqualityComparer<Color32>
        {
            public bool Equals(Color32 c1, Color32 c2)
            {
                return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
            }

            public int GetHashCode(Color32 c)
            {
                return c.GetHashCode();
            }
        }

        public class RevertContainer<T>
        {
            public RevertContainer(int max)
            {
                capacity = max;
            }

            public void Push(T item)
            {
                container.Insert(0, item);
            }

            public T Peek()
            {
                if (container.Count > 0)
                    return container[0];
                else
                    return default(T);
            }

            public T Pop()
            {
                return Remove(0);
            }

            public T Drop()
            {
                return Remove(container.Count - 1);
            }

            private T Remove(int index)
            {
                if (container.Count > 0)
                {
                    T temp = container[index];
                    container.RemoveAt(index);
                    return temp;
                }
                else { return default(T); }
            }

            public void Clear()
            {
                container.Clear();
            }

            public int Count()
            {
                return container.Count;
            }

            public int GetCapacity()
            {
                return capacity;
            }

            private readonly int capacity;
            private List<T> container = new();
        }

        public static bool IsMouseOverUI()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);
            int UILayer = LayerMask.NameToLayer("Static UI");

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.layer == UILayer)
                    return true;
            }
            return false;
        }

        public static bool IsPointOnScreen(int x, int y, int screenW, int screenH)
        {
            return (x >= 0 && x < screenW && y >= 0 && y < screenH);
        }

        public static ulong C32toulong(Color32 color)
        {
            return color.r + (color.g * GREEN_CONST) + (color.b * BLUE_CONST) + (color.a * ALPHA_CONST) ;
        }

        public static uint C32toi(Color32 color)
        {
            return (uint)(color.r | color.g << 8 | color.b << 16 | color.a << 24);
        }

        public static bool C32compare(Color32 c1, Color32 c2)
        {
            return C32toi(c1) == C32toi(c2);
        }

        public static bool C32compare(Color32 c1, byte red, byte blue, byte green)
        {
            return (c1.r == red && c1.g == green && c1.b == blue);
        }

        public static bool C32compare(Color32 c1, byte red, byte blue, byte green, byte alpha)
        {
            return (c1.r == red && c1.g == green && c1.b == blue && c1.a == alpha);
        }

        public static float DistanceCalc(float x1, float y1, float x2, float y2)
        {
            float dX2 = Mathf.Pow(x1 - x2, 2);
            float dY2 = Mathf.Pow(y1 - y2, 2);
            return Mathf.Sqrt(dX2 + dY2);
        }

        public static bool Point2DInBounds(Vector3[] bounds, Vector2 point)
        {
            if (bounds.Length != 4)
                return false;

            return (point.x >= bounds[0].x && point.x <= bounds[2].x && point.y >= bounds[0].y && point.y <= bounds[2].y);
        }

        public static bool Point2DInRect(int width, int height, Vector2 point)
        {
            return (point.x >= 0 && point.x < width && point.y >= 0 && point.y < height);
        }

        public static Vector2 RectRelativePos(Vector3[] bounds, Vector2 point)
        {
            return new Vector2(point.x - bounds[0].x, point.y - bounds[0].y);
        }
    }
}