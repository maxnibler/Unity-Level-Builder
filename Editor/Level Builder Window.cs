using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace LB
{
    public class LevelBuilder : EditorWindow
    {
        private int _gridHeight = 10;
        private int _gridWidth = 10;
        private VisualElement[,] _grid;
        private Ajacents[,] _adjacents;
        private bool _active = false;
        private Label _contents;
        public VisualElement Window;
        private SliderInt _heightSlider;
        private SliderInt _widthSlider;
        private FloatField _tileSizeField;
        private Color _floorColor = Color.grey;
        private Color _emptyColor;
        private Color _wallColor = Color.black;
        private ObjectField _tileSet;
        private Painting _current;

        private const int c_gridSize = 50; // this can be updated directly if you need a larger grid

        [MenuItem("Window/Level Builder")]
        public static void ShowWindow()
        {
            LevelBuilder w = (LevelBuilder) EditorWindow.GetWindow(typeof(LevelBuilder));
        }

        public void CreateGUI()
        {
            this.rootVisualElement.Clear();
            this.Activate();
        }

        void OnGUI()
        {
            if (!_active) return;
            if (_gridHeight != _heightSlider.value) updateHeight();
            if (_gridWidth != _widthSlider.value) updateWidth();
        }

        public void Activate()
        {
            _floorColor = Color.black;
            _wallColor = Color.blue;
            _emptyColor = Color.grey;

            VisualTreeAsset windowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.maxnibler.levelbuilder/UI Elements/Window.uxml");

            this.Window = windowAsset.Instantiate();

            _contents = new Label("x");

            this.rootVisualElement.Add(this.Window);
            
            _tileSizeField = Window.Q<FloatField>(name: "tile_size");
            setupSliders();
            setupPainter();

            Button clearButton = Window.Q<Button>(name: "clear_grid");
            clearButton.clicked += clearGrid;

            VisualElement insertPanel = this.Window.Q<VisualElement>(name:"insert_panel");

            setupObjectFields(insertPanel);

            VisualElement contents = this.Window.Q<VisualElement>(name: "content_box");
            contents.Add(_contents);
            initGrid();
            Button generateButton = this.Window.Q<Button>(name: "generate_button");
            generateButton.clicked += generateScene;
            _active = true;
        }

        private void setupPainter()
        {
            _current = Painting.Wall;

            Button wallButton = Window.Q<Button>(name: "paint_wall");
            wallButton.style.backgroundColor = _wallColor;
            wallButton.clicked += () => 
            {
                _current = Painting.Wall;
            };

            Button floorButton = Window.Q<Button>(name: "paint_floor");
            floorButton.style.backgroundColor = _floorColor;
            floorButton.clicked += () =>
            {
                _current = Painting.Floor;
            };

            Button emptyButton = Window.Q<Button>(name: "paint_empty");
            emptyButton.style.backgroundColor = _emptyColor;
            emptyButton.clicked += () =>
            {
                _current = Painting.Empty;
            };
        }

        private void setupSliders()
        {
            this._heightSlider = this.Window.Q<SliderInt>(name: "h_slider");
            this._widthSlider = this.Window.Q<SliderInt>(name: "w_slider");

            _heightSlider.highValue = c_gridSize;
            _widthSlider.highValue = c_gridSize;
            _heightSlider.value = 10;
            _widthSlider.value = 10;
        }

        private void setupObjectFields(VisualElement insertPanel)
        {
            _tileSet = new ObjectField("Tileset");
            _tileSet.objectType = typeof(Tileset);
            insertPanel.Add(_tileSet);
            Tileset tileset = AssetDatabase.LoadAssetAtPath<Tileset>("Packages/com.maxnibler.levelbuilder/Default Pieces/Default Tileset.asset");
            _tileSet.value = tileset; 
        }

        private void clearGrid()
        {
            for (int i=0; i<c_gridSize; i++)
            {
                for (int j=0; j<c_gridSize; j++)
                {
                    _grid[i,j].style.backgroundColor = _emptyColor;
                    _adjacents[i,j].Reset();
                }
            }
        }

        private void initGrid()
        {
            VisualElement contents = this.Window.Q<VisualElement>(name: "content_box");
            VisualTreeAsset rowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.maxnibler.levelbuilder/UI Elements/Row.uxml");

            contents.Clear();
            _grid = new VisualElement[c_gridSize, c_gridSize];
            _adjacents = new Ajacents[c_gridSize, c_gridSize];
            for (int i=0; i<c_gridSize; i++)
            {
                VisualElement row = rowAsset.Instantiate();
                contents.Add(row);
                row = row.Q<VisualElement>(name: "row");
                for (int j=0; j<c_gridSize; j++)
                {
                    _adjacents[i,j] = new Ajacents(0);
                    addGridItem(i, j, row);
                }
            }
        }
        private void updateWidth()
        {
            _gridWidth = _widthSlider.value;
            updateContents();
        }
        private void updateHeight()
        {
            _gridHeight = _heightSlider.value;
            updateContents();
        }

        private void updateContents()
        {
            for (int i=0; i<c_gridSize; i++)
            {
                for (int j=0; j<c_gridSize; j++)
                {
                    setGridVisibility(i, j);
                }
            }
        }

        private void setGridVisibility(int height, int width)
        {
            if (height < _gridHeight && width < _gridWidth)
            {
                _grid[height, width].style.visibility = Visibility.Visible;
                return;
            }

            _grid[height, width].style.visibility = Visibility.Hidden;
        }

        private void addGridItem(int i, int j, VisualElement row)
        {
            Button gridItem = new Button();
            gridItem.text = string.Format("X");
            gridItem.style.backgroundColor = _emptyColor;
            gridItem.clicked += () => {
                gridClicked(i, j);
            };
            _grid[i,j] = gridItem;
            row.Add(gridItem);
            gridItem.style.visibility = Visibility.Hidden;
        }

        private void gridClicked(int i, int j)
        {
            bool wall = changeColor((Button) _grid[i,j]);
            setAdjacents(i, j, wall);
        }

        private bool changeColor(Button button)
        {
            switch (_current)
            {
                case Painting.Wall:
                    button.style.backgroundColor = _wallColor;
                    return true;
                case Painting.Floor:
                    button.style.backgroundColor = _floorColor;
                    return false;
                case Painting.Empty:
                    button.style.backgroundColor = _emptyColor;
                    return false;
            }
            return false;
        }

        private void setAdjacents(int i, int j, bool wall)
        {
            if (inBounds(i-1, j))
            {
                _adjacents[i-1,j].Right = wall;
            }
            if (inBounds(i+1, j))
            {
                _adjacents[i+1,j].Left = wall;
            }
            if (inBounds(i, j-1))
            {
                _adjacents[i,j-1].Down = wall;
            }
            if (inBounds(i, j+1))
            {
                _adjacents[i,j+1].Up = wall;
            }
        }

        private bool inBounds(int i, int j)
        {
            if (i < 0) return false;
            if (j < 0) return false;
            if (i > 99) return false;
            if (j > 99) return false;
            return true;
        }

        private void generateScene()
        {
            GameObject levelContainer = new GameObject("Level Build");
            for (int i=0; i<c_gridSize; i++)
            {
                for (int j=0; j<c_gridSize; j++)
                {
                    placeWall(i, j, levelContainer, _grid[i,j].style.backgroundColor == _wallColor);
                    placeFloor(i, j, levelContainer, _grid[i,j].style.backgroundColor == _floorColor);
                }
            }
        }

        private void placeFloor(int i, int j, GameObject container, bool floor)
        {
            if (!floor) return;
            
            Tileset tileSet = (Tileset) _tileSet.value;
            Tile tile = tileSet.GetTileByType(WP.Floor);
            Vector3 position = new Vector3(tileSet.TileSize * i, 0, tileSet.TileSize * j);
            GameObject go = placeTile(tile, position);
            go.transform.Rotate(Vector3.up, tile.Rotations * 90, Space.Self);
            go.transform.parent = container.transform;
        }

        private void placeWall(int i, int j, GameObject container, bool wall)
        {
            if (!wall) return;

            WP pieceEnum = selectWP(_adjacents[i,j]);
            
            Tileset tileSet = (Tileset) _tileSet.value;
            Tile tile = tileSet.GetTileByType(pieceEnum);

            Vector3 position = new Vector3(tileSet.TileSize * i, 0, tileSet.TileSize * j);
            GameObject go = placeTile(tile, position);

            int rotation = getRotation(pieceEnum, i, j);
            go.transform.Rotate(Vector3.up, rotation + tile.Rotations * 90, Space.Self);
            go.transform.parent = container.transform;
        }

        private GameObject placeTile(Tile tile, Vector3 position)
        {
            GameObject prefab = tile.prefab;
            GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = position;
            return go;
        }

        private WP selectWP(Ajacents a)
        {
            switch (a.Count)
            {
                case 0:
                    return WP.Column;
                case 1:
                    return WP.End;
                case 2:
                    if (lineWall(a)) return WP.Line;
                    return WP.Corner;
                case 3:
                    return WP.T;
                case 4:
                    return WP.X;
                default:
                    Debug.LogError(string.Format("{0} number of adjacents should not be possible", a.Count));
                    break;
            }
            return 0;
        }


        private bool lineWall(Ajacents a)
        {
            if (a.Up && a.Down) return true;
            if (a.Right && a.Left) return true;
            return false;
        }

        private int getRotation(WP piece, int i, int j)
        {
            Ajacents a = _adjacents[i, j];
            switch (piece)
            {
                case WP.Column:
                    return 0;
                case WP.End:
                    return endWallDirection(a);
                case WP.Line:
                    return lineWallDirection(a);
                case WP.Corner:
                    return cornerWallDirection(a);
                case WP.T:
                    return tWallDirection(a);
                case WP.X:
                    return xWallDirection(a);
                default:
                    return 0;
            }
        }

        private int endWallDirection(Ajacents a)
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

        private int lineWallDirection(Ajacents a)
        {
            if (a.Up)
            {
                return 90;
            }
            return 0;
        }

        private int cornerWallDirection(Ajacents a)
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

        private int tWallDirection(Ajacents a)
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

        private int xWallDirection(Ajacents a)
        {
            return 0;
        }
        private struct Ajacents
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

        private enum Painting
        {
            Wall,
            Floor,
            Empty,
        }
    }
}