using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.InteropServices;

namespace BookLib
{
    public class ImgTools : MonoBehaviour
    {
        /// 
        /// CURRENT TODO:
        /// Clean up code, remove rudundancies, optimize as much as possible.
        /// Make as readable as possible for future editors.
        /// Look at implementing more tools.
        /// Clean up the image shrinking, it is a little crusty.
        /// Combine with rodney's UI.
        /// Thoroughly test on webgl.
        ///
        /// THE LAST KNOWN PLACE OF PERFORMANCE LOSS IS THE HIGHLIGHTER TOOL
        /// Look at computation time and see what can be improved, to avoid frame drops.
        /// 
        /// Some buttons like REDO can cause errors
        ///

        private const int POINTS_DIVISOR = 4;
        private static Color32 STENCIL_COLOR = new Color32(1, 1, 1, 255);
        private const int REVERT_CAPACITY = 10;

        // RawImage that contains the drawn-on image, which can be downloaded and is shown on screen
        [SerializeField] private RawImage finalDrawnImage;
        // PlayerController handles the brush, brush resizing, and color picking
        [SerializeField] private PlayerController playerControl;
        // Indicator that changes colors with what the player selects
        [SerializeField] private ColorIndicator indicator;
        // List of GameObjects that have images, which will be drawn on like a stencil
        [SerializeField] private List<GameObject> stencils;
        private int activeStencil;

        // Raw pixel color data array, 1 dimension indexed 2 dimensionally
        private Color32[] colorMap;
        // Array of the same color data represented as integer IDs, used to compare color signatures
        private uint[] idMap;
        // Hashmap of uint IDs to colors
        private Dictionary<uint, Color32> currentHash;
        // Hashmap of stencil ID to pixel-by-pixel alpha bytes
        private Dictionary<int, List<BookLib.IntBytePair>> stencilIndexDict;
        // List of all textures, so that the drawings are preserved when switching stencils
        private Dictionary<int, Texture2D> allTextures;
        // Current drawing texture, which is passed to the finalDrawnImage for display and downloading
        private Texture2D mainTexture;
        // Lists of Undo and Redo textures, so the user can flip back and forth between brush strokes and changes
        private BookLib.RevertContainer<Texture2D> undoTextures;
        private BookLib.RevertContainer<Texture2D> redoTextures;

        // Current image width and height
        private int imgWidth;
        private int imgHeight;
        // Bounds on the image on the screen
        private Vector3[] imgBoundsScreen;
        // Bounds of the screen NOT INCLUDING UI, only drawable area
        private Vector2 screenBounds;

        // Currently active drawing tool, color, and color/brush data
        [SerializeField] private Tools activeTool;
        private Color32 selectedColor;
        private uint selectedColorCode;
        private int alpha;
        private int brushSize;
        // Tracks if the current brush stroke is still valid or not, to prevent drawing under UI
        private bool validStroke;

        // Previous position of the mouse, so that a brush stroke can be drawn between this and current position
        private Vector3 mousePosPrev;

        public enum Tools
        {
            FillBucket,
            Highlighter,
            Brush,
            Eraser
        }

        // Import jslib file to handle downloading on website
        [DllImport("__Internal")]
        private static extern void DownloadFile(byte[] bytes, int length, string name);

        private void Start()
        {
            if (!finalDrawnImage)
                finalDrawnImage = GameObject.FindWithTag("RawImage").GetComponent<RawImage>();
            if (!playerControl)
                playerControl = GameObject.FindWithTag("PlayerController").GetComponent<PlayerController>();
            if (!indicator)
                indicator = GameObject.FindWithTag("ColorIndicator").GetComponent<ColorIndicator>();

            activeTool = Tools.Brush;
            activeStencil = 0;
            undoTextures = new(REVERT_CAPACITY);
            redoTextures = new(REVERT_CAPACITY);
            SetAlpha(playerControl.GetAlpha());
            SetSelectedColor(playerControl.GetColor());

            imgBoundsScreen = new Vector3[4];
            finalDrawnImage.rectTransform.GetWorldCorners(imgBoundsScreen);
            for (int i = 0; i < imgBoundsScreen.Length; i++)
            {
                imgBoundsScreen[i] = Camera.main.WorldToScreenPoint(imgBoundsScreen[i]);
            }
            screenBounds = BookLib.RectRelativePos(imgBoundsScreen, new Vector2(Screen.width, Screen.height));
            imgWidth = finalDrawnImage.GetComponent<RawImage>().texture.width;
            imgHeight = finalDrawnImage.GetComponent<RawImage>().texture.height;
            Debug.Log(imgWidth + " " + imgHeight);

            allTextures = new Dictionary<int, Texture2D>();
            stencilIndexDict = new Dictionary<int, List<BookLib.IntBytePair>>();
            stencils[activeStencil].SetActive(true);
            for (int i = 0; i < stencils.Count; i++)
            {
                activeStencil = i;
                stencils[activeStencil].SetActive(true);
                ScreenFill(new Color32(255, 255, 255, 255));
                GenerateStencilMap();
                stencils[activeStencil].SetActive(false);
            }
            activeStencil = 0;
            mainTexture = allTextures[0];
            StampStencil();

            stencils[activeStencil].SetActive(false);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                validStroke = false; // default

            switch (activeTool)
            {
                case (Tools.Brush):
                    BrushTool(255, selectedColor);
                    break;
                case (Tools.Highlighter):
                    HighlighterTool();
                    break;
                case (Tools.FillBucket):
                    FillBucketTool();
                    break;
                case (Tools.Eraser):
                    EraserTool();
                    break;
            }

            mousePosPrev = BookLib.RectRelativePos(imgBoundsScreen, Input.mousePosition);
        }

        private void ScreenFill(Color32 fillColor) // Fills entire screen with given color
        {
            colorMap = new Color32[imgWidth * imgHeight];
            idMap = new uint[imgWidth * imgHeight];
            mainTexture = new Texture2D(imgWidth, imgHeight, TextureFormat.RGBA32, false);
            for (int i = 0; i < imgWidth * imgHeight; i++)
            {
                colorMap[i] = fillColor;
                idMap[i] = BookLib.C32toi(fillColor);
            }
            mainTexture.SetPixels32(colorMap);
            mainTexture.Apply();
            allTextures[activeStencil] = mainTexture;
            finalDrawnImage.texture = mainTexture;
        }

        // Generates a map of which pixels in the stencil are non-background,
        // so that the relevant pixels can be "stamped" onto the drawing area.
        private void GenerateStencilMap()
        {
            // Get raw image, get stencil texture
            RawImage rawImage = stencils[activeStencil].GetComponent<RawImage>();
            Texture2D stencilTex = (Texture2D)rawImage.texture;
            stencilTex.Apply();
            
            // Get pixels from stencil texture, and derive background color from them
            Color32[] tempMap;
            tempMap = stencilTex.GetPixels32();
            Color32 backgroundColor = tempMap[0];

            // Iterate over the stencil texture, and mark which pixels are background or not
            // by logging what alpha they should be
            stencilIndexDict[activeStencil] = new List<BookLib.IntBytePair>();
            for (int i = 0; i < imgWidth * imgHeight; i++)
            {
                if (!tempMap[i].Equals(backgroundColor))
                    stencilIndexDict[activeStencil].Add(new BookLib.IntBytePair(i, tempMap[i].a));
            }
        }

        private void StampStencil()
        {
            List<BookLib.IntBytePair> indexList = stencilIndexDict[activeStencil];
            for (int i = 0; i < indexList.Count; i++)
            {
                Color32 indexColor = new(STENCIL_COLOR.r, STENCIL_COLOR.g, STENCIL_COLOR.b, indexList[i].Byte);
                colorMap[indexList[i].Int] = indexColor;
                idMap[indexList[i].Int] = BookLib.C32toi(indexColor); // Used to be locked to 255 alpha, may cause issues not being so.
            }

            mainTexture.SetPixels32(colorMap);
            mainTexture.Apply();
            allTextures[activeStencil] = mainTexture;
            finalDrawnImage.texture = mainTexture;
        }

        private void BrushTool(int a, Color32 color)
        {
            SetAlpha(a);

            if (Input.GetMouseButtonDown(0) && !BookLib.IsMouseOverUI())
            {
                validStroke = true;

                redoTextures.Clear();
                SetUndoTexture();

                mousePosPrev = BookLib.RectRelativePos(imgBoundsScreen, Input.mousePosition);
                currentHash = null;
                currentHash = new Dictionary<uint, Color32>();

                DrawNoShader(BookLib.RectRelativePos(imgBoundsScreen, Input.mousePosition), color);
                StampStencil();
            }
            else if (Input.GetMouseButton(0) && validStroke)//&& !BookLib.IsMouseOverUI())
            {
                DrawNoShader(BookLib.RectRelativePos(imgBoundsScreen, Input.mousePosition), color);
                StampStencil();
            }
        }

        private void HighlighterTool()
        {
            BrushTool(128, selectedColor);
        }

        private void FillBucketTool()
        {
            if (Input.GetMouseButtonDown(0) && !BookLib.IsMouseOverUI())
            {
                redoTextures.Clear();
                SetUndoTexture();
                SetAlpha(255);
                StartCoroutine(StartScanLineFill(BookLib.RectRelativePos(imgBoundsScreen, Input.mousePosition)));
            }
        }

        private void EraserTool()
        {
            BrushTool(255, new Color32(255, 255, 255, 255));
        }


        private void DrawNoShader(Vector2 mousePos, Color32 color)
        {
            BookLib.Int2 mousePosCur = new(0, 0);
            mousePosCur.x = (int)(mousePos.x * (imgWidth / screenBounds.x));
            mousePosCur.y = (int)(mousePos.y * (imgHeight / screenBounds.y));

            BookLib.Int2 mousePosPrevInt = new(0, 0);
            mousePosPrevInt.x = (int)(mousePosPrev.x * (imgWidth / screenBounds.x));
            mousePosPrevInt.y = (int)(mousePosPrev.y * (imgHeight / screenBounds.y));

            int pointsCount = (int)Mathf.Ceil(BookLib.DistanceCalc(mousePosCur.x, mousePosCur.y, mousePosPrevInt.x, mousePosPrevInt.y) / POINTS_DIVISOR);
            List<BookLib.Int2> points = new();
            points.Add(mousePosCur);

            float deltaT = 1f / pointsCount;
            float t = deltaT;
            for (int i = 0; i < pointsCount; i++)
            {
                points.Add(new BookLib.Int2((int)Mathf.Round(Mathf.Lerp(mousePosCur.x, mousePosPrevInt.x, t)),
                                        (int)Mathf.Round(Mathf.Lerp(mousePosCur.y, mousePosPrevInt.y, t))));
                t += deltaT;
            }

            points = points.Distinct().ToList();
            pointsCount = points.Count();

            for (int i = 0; i < pointsCount; i++)
            {
                int upperBound = points[i].y + brushSize;
                if (upperBound > imgHeight) { upperBound = imgHeight; }
                    
                int rightBound = points[i].x + brushSize;
                if (rightBound > imgWidth) { rightBound = imgWidth; }

                // Color center of point first, to maintain accurate colors, if alpha blending
                if (alpha < 255 && BookLib.Point2DInRect(imgWidth, imgHeight, new Vector2(points[i].x, points[i].y)))
                    AlphaPixel(points[i].x + (imgWidth * points[i].y), color);

                // Start from center of point - brush size, for brushsize pixels beneath point
                for (int y = points[i].y - brushSize; y < upperBound; y++)
                {
                    // If off rect, set to 1 pixel below rect
                    if (y < 0)
                    {
                        y = -1; // because the loop will increment by 1
                        continue;
                    }

                    // Start from brushSize pixels left of point, set to left side of rect if off of it
                    int leftBound = points[i].x - brushSize;
                    if (leftBound < 0) { leftBound = 0; }
                    int x = leftBound;
                    while (x < rightBound && BookLib.DistanceCalc(points[i].x, points[i].y, x, y) > brushSize) { x++; } // Increment until pixel is in radius
                    int xMin = x; // This is leftmost close enough pixel

                    x = rightBound - 1; // Start from right side and repeat process to the left
                    while (x > leftBound && BookLib.DistanceCalc(points[i].x, points[i].y, x, y) > brushSize) { x--; }
                    int xMax = x; // This is rightmost close enough pixel

                    // Between and including those pixels, change colors to correct output
                    for (x = xMin; x <= xMax; x++)
                    {
                        int index = x + (imgWidth * y);
                        if (alpha == 255) // Solid brush
                        {
                            colorMap[index] = color;
                            colorMap[index].a = 255;
                            idMap[index] = BookLib.C32toi(color);
                            continue;
                        }

                        // Brush with alpha (eg. highlighter)
                        AlphaPixel(index, color);
                    }
                }
            }
            mousePosPrev = Input.mousePosition;
        }

        private void AlphaPixel(int index, Color32 color)
        {
            uint hashedID = idMap[index];
            if (!currentHash.ContainsKey(hashedID))
            {
                byte a = (byte)(255 - ((255 - colorMap[index].a) * (255 - alpha) / 255));
                byte r = (byte)((colorMap[index].r * (255 - alpha) + color.r * alpha) / 255);
                byte g = (byte)((colorMap[index].g * (255 - alpha) + color.g * alpha) / 255);
                byte b = (byte)((colorMap[index].b * (255 - alpha) + color.b * alpha) / 255);
                Color32 combinedColor = new(r, g, b, a);
                currentHash.Add(hashedID, combinedColor); // Original color ID => Blended color
                currentHash.TryAdd(BookLib.C32toi(combinedColor), combinedColor); // Blended color ID => Blended color
                // ^ Is TryAdd because, example, if you highlight over a highlighted color, and then white, it will cause errors with just Add
                //      due to the white trying to add the color you originally highlighted over.
                //      I have not found a better solution, everything else looks janky, but it is a work in progress.
            }

            colorMap[index] = currentHash[hashedID];
            idMap[index] = BookLib.C32toi(currentHash[hashedID]);
        }

        public IEnumerator StartScanLineFill(Vector2 mousePos)
        {
            StampStencil();

            int x = (int)(mousePos.x * (imgWidth / screenBounds.x));
            int y = (int)(mousePos.y * (imgHeight / screenBounds.y));
            if (!BookLib.Point2DInBounds(imgBoundsScreen, Input.mousePosition))
                yield break;

            Color32 oldColor = colorMap[x + (imgWidth * y)];
            if (oldColor.r == selectedColor.r && oldColor.g == selectedColor.g && oldColor.b == selectedColor.b)
            { // For some reason oldColor.Equals(selectedColor) was not working specifically for white, so this is the "fix"
                yield break;
            }

            uint oldColorCode = BookLib.C32toi(oldColor);
            if (idMap[x + (imgWidth * y)] == oldColorCode)
                ScanLineFill(x, y, oldColorCode, oldColor);

            StampStencil();
        }

        private void ScanLineFill(int x, int y, uint oldColorCode, Color32 oldColor)
        {
            while (true)
            {
                int originalX = x, originalY = y;
                while (y != 0 && idMap[x + (imgWidth * (y - 1))] == oldColorCode)
                    y--;
                while (x != 0 && idMap[(x - 1) + (imgWidth * y)] == oldColorCode)
                    x--;

                if (x == originalX && y == originalY)
                    break;
            }
            _ScanLineFill(x, y, oldColorCode, oldColor);
        }

        private void _ScanLineFill(int x, int y, uint oldColorCode, Color32 oldColor)
        {
            int lastRowLen = 0;
            do
            {
                int rowLen = 0, sx = x;

                if (lastRowLen != 0 && idMap[x + (imgWidth * y)] != oldColorCode)
                {
                    do
                    {
                        if (--lastRowLen == 0)
                            return;
                    } while (idMap[++x + (imgWidth * y)] != oldColorCode);
                    sx = x;
                }
                else
                {
                    for (; x != 0 && idMap[(x - 1) + (imgWidth * y)] == oldColorCode; rowLen++, lastRowLen++)
                    {
                        x--;
                        idMap[x + (imgWidth * y)] = selectedColorCode;
                        colorMap[x + (imgWidth * y)] = selectedColor;
                        colorMap[x + (imgWidth * y)].a = (byte)alpha;

                        if (y != 0 && idMap[x + (imgWidth * (y - 1))] == oldColorCode)
                            ScanLineFill(x, y - 1, oldColorCode, oldColor);
                    }
                }

                for (; sx < imgWidth && idMap[sx + (imgWidth * y)] == oldColorCode; rowLen++, sx++)
                {
                    idMap[sx + (imgWidth * y)] = selectedColorCode;
                    colorMap[sx + (imgWidth * y)] = selectedColor;
                    colorMap[sx + (imgWidth * y)].a = (byte)alpha;
                }

                if (rowLen < lastRowLen)
                {
                    for (int lastEnd = x + lastRowLen; ++sx < lastEnd;)
                    {
                        if (idMap[sx + (imgWidth * y)] == oldColorCode)
                            _ScanLineFill(sx, y, oldColorCode, oldColor);
                    }
                }
                else if (rowLen > lastRowLen && y != 0)
                {
                    for (int i = x + lastRowLen; ++i < sx;)
                    {
                        if (idMap[i + (imgWidth * (y - 1))] == oldColorCode)
                            _ScanLineFill(i, y - 1, oldColorCode, oldColor);
                    }
                }

                lastRowLen = rowLen;
            } while (lastRowLen != 0 && ++y < imgHeight);
        }

        public void GenerateIDMap()
        {
            for (int i = 0; i < imgWidth * imgHeight; i++)
            {
                idMap[i] = BookLib.C32toi(colorMap[i]);
            }
        }

        public void DownloadImage()
        {
            byte[] texBytes = mainTexture.EncodeToPNG();
            DownloadFile(texBytes, texBytes.Length, "drawing.png");
        }

        /*private void Revert()
        {
            colorMap = mainTexturePrev.GetPixels32();
            Color32[] tempMap = new Color32[imgWidth * imgHeight];
            tempMap = mainTexture.GetPixels32();
            mainTexturePrev.SetPixels32(tempMap);
            GenerateIDMap();
            mainTexture.SetPixels32(colorMap);
            mainTexture.Apply();

            tempMap = null;

            revertState = revertState == 0 ? (byte)1 : (byte)0;
        }*/

        public void Undo()
        {
            if (undoTextures.Peek() == default(Texture2D))
                return;
            
            SetRedoTexture();
            colorMap = undoTextures.Pop().GetPixels32();
            GenerateIDMap();
            mainTexture.SetPixels32(colorMap);
            mainTexture.Apply();
        }

        public void Redo()
        {
            if (redoTextures.Peek() == default(Texture2D))
                return;

            SetUndoTexture();
            colorMap = redoTextures.Pop().GetPixels32();
            GenerateIDMap();
            mainTexture.SetPixels32(colorMap);
            mainTexture.Apply();
        }

        private void CheckSetUndoValid()
        {
            if (BookLib.Point2DInBounds(imgBoundsScreen, Input.mousePosition))
            {
                redoTextures.Clear();
                SetUndoTexture();
            }
        }

        private void SetUndoTexture()
        {
            Texture2D temp = new(mainTexture.width, mainTexture.height, mainTexture.format, false);
            temp.SetPixels32(colorMap);

            if (undoTextures.Count() == undoTextures.GetCapacity())
                undoTextures.Drop();

            undoTextures.Push(temp);
        }

        private void SetRedoTexture()
        {
            Texture2D temp = new(mainTexture.width, mainTexture.height, mainTexture.format, false);
            temp.SetPixels32(colorMap);

            if (redoTextures.Count() == redoTextures.GetCapacity())
                redoTextures.Drop();

            redoTextures.Push(temp);
        }

        public void NextStencil()
        {
            undoTextures.Clear();
            redoTextures.Clear();
            if (++activeStencil >= stencils.Count) { activeStencil = 0; }
            mainTexture = allTextures[activeStencil];
            colorMap = mainTexture.GetPixels32();
            GenerateIDMap();
            StampStencil();
        }
        public void PreviousStencil()
        {
            undoTextures.Clear();
            redoTextures.Clear();
            if (--activeStencil < 0) { activeStencil = stencils.Count - 1; }
            mainTexture = allTextures[activeStencil];
            colorMap = mainTexture.GetPixels32();
            GenerateIDMap();
            StampStencil();
        }

        public void SetSelectedColor(Color32 color)
        {
            selectedColor = color;
            selectedColorCode = BookLib.C32toi(color);
            indicator.SetColor(color);
        }

        public Color32 GetSelectedColor()
        {
            return selectedColor;
        }

        public void SetTool(Tools tool)
        {
            activeTool = tool;
        }

        public Tools GetTool()
        {
            return activeTool;
        }

        public void SetAlpha(int a)
        {
            alpha = a;
        }

        public void SetSize(int s)
        {
            brushSize = s;
        }
    }
}