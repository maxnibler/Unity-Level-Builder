using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;

namespace LB
{
    public class LevelBuilder : EditorWindow
    {
        private int _gridHeight = 10;
        private int _gridWidth = 10;
        private VisualElement[,] _grid;
        private Ajacents[,] _adjacents;
        private bool _active = false;
        public VisualElement Window;
        private SliderInt _heightSlider;
        private SliderInt _widthSlider;
        private FloatField _tileSizeField;
        private Color _floorColor = Color.grey;
        private Color _emptyColor;
        private Color _wallColor = Color.black;
        private Color _doorColor = Color.black;
        private Color _columnColor = Color.black;
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
            _emptyColor = Color.clear;
            _doorColor = Color.magenta;
            _columnColor = Color.grey;

            VisualTreeAsset windowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.maxnibler.levelbuilder/UI Elements/Window.uxml");

            this.Window = windowAsset.Instantiate();


            this.rootVisualElement.Add(this.Window);
            
            _tileSizeField = Window.Q<FloatField>(name: "tile_size");
            setupSliders();
            setupPainter();

            Button clearButton = Window.Q<Button>(name: "clear_grid");
            clearButton.clicked += clearGrid;

            VisualElement insertPanel = this.Window.Q<VisualElement>(name:"insert_panel");

            setupObjectFields(insertPanel);

            Button saveButton = new Button();
            saveButton.clicked += save;
            saveButton.text = "Save current Level";
            insertPanel.Add(saveButton);
            
            Button loadButton = new Button();
            loadButton.clicked += open;
            loadButton.text = "Load Saved Level";
            insertPanel.Add(loadButton);
            
            VisualElement contents = this.Window.Q<VisualElement>(name: "content_box");
            initGrid();
            Button generateButton = this.Window.Q<Button>(name: "generate_button");
            generateButton.clicked += generateScene;
            _active = true;
        }

        private void save()
        {
            string path = SaveDataUtil.SavePath();
            if (path == "") return;
            int [,] arrayRepresentation = getCurrentArray();
            SaveDataUtil.SaveArrayToPath(arrayRepresentation, path);
        }

        private void open()
        {
            string path = SaveDataUtil.OpenPath();
            if (path == "") return;
            string [] fileData = SaveDataUtil.OpenFileAtPath(path);
            loadLayoutFromArray(fileData);
        } 

        private void loadLayoutFromArray(string [] arr)
        {
            for (int i=0; i<arr.GetLength(0); i++)
            {
                string [] row = arr[i].Split(",");
                for (int j=0; j<row.GetLength(0); j++)
                {
                    Painting tile = paintingFromString(row[j]);
                    setTileAt(i, j, tile);
                }
                // Debug.Log(string.Format("{0}: {1}", i, arr[i]));
            }
        }

        private int [,] getCurrentArray()
        {
            int [,] arrayRep = new int [c_gridSize,c_gridSize];
            for (int i=0; i<c_gridSize; i++)
            {
                for (int j=0; j<c_gridSize; j++)
                {
                    arrayRep[i,j] = (int) getTileType(i,j);
                }
            }
            return arrayRep;
        }

        private Painting getTileType(int i, int j)
        {
            StyleColor c = _grid[i,j].style.backgroundColor; 
            if (c==_wallColor) return Painting.Wall;
            if (c==_floorColor) return Painting.Floor;
            return Painting.Empty;
        }

        private void setTileAt(int i, int j, Painting tileType)
        {
            _current = tileType;
            bool wall = changeColor((Button) _grid[i,j]);
            setAdjacents(i, j, wall);
        }

        private Painting paintingFromString(string s)
        {
            try
            {
                int i = Int32.Parse(s);
                return (Painting) i;
            }
            catch (FormatException)
            {
                Debug.LogError(string.Format("{0} is not a valid int", s));
            }
            return Painting.Empty;
        }

        private void setupPainter()
        {
            _current = Painting.Wall;

            GetSetButton("paint_empty", _emptyColor, Painting.Empty);
            GetSetButton("paint_wall", _wallColor, Painting.Wall);
            GetSetButton("paint_floor", _floorColor, Painting.Floor);
            GetSetButton("paint_door", _doorColor, Painting.Door);
            GetSetButton("paint_column", _columnColor, Painting.Column);
        }

        private void GetSetButton(string name, Color color, Painting p)
        {
            Button b = Window.Q<Button>(name: name);
            b.style.backgroundColor = color;
            b.clicked += () =>
            {
                _current = p;
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
            Tileset tileset = AssetDatabase.LoadAssetAtPath<Tileset>("Packages/com.maxnibler.levelbuilder/Resources/Default Pieces/Default Tileset.asset");
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
                case Painting.Column:
                    button.style.backgroundColor = _columnColor;
                    return false;
                case Painting.Door:
                    button.style.backgroundColor = _doorColor;
                    return true;
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
            if (i > c_gridSize-1) return false;
            if (j > c_gridSize-1) return false;
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
                    placeColumn(i, j, levelContainer, _grid[i,j].style.backgroundColor == _columnColor);
                    placeDoor(i, j, levelContainer, _grid[i,j].style.backgroundColor == _doorColor);
                }
            }
        }

        private void placeDoor(int i, int j, GameObject container, bool door)
        {
            if (!door) return;

            if (!validDoor(i,j))
            {
                placeWall(i,j,container,true);
                return;
            }

            Tileset tileSet = (Tileset) _tileSet.value;
            Tile tile = tileSet.GetTileByType(WP.Door);

            Vector3 position = new Vector3(tileSet.TileSize * i, 0, tileSet.TileSize * j);
            GameObject go = placeTile(tile, position);

            int rotation = getRotation(WP.Line, i, j);
            go.transform.Rotate(Vector3.up, rotation + tile.Rotations * 90, Space.Self);
            go.transform.parent = container.transform;
        }

        private bool validDoor(int i, int j)
        {
            return _adjacents[i,j].Count == 2;
        }

        private void placeColumn(int i, int j, GameObject container, bool column)
        {
            if (!column) return;

            Tileset tileSet = (Tileset) _tileSet.value;
            Tile tile = tileSet.GetTileByType(WP.Column);

            Vector3 position = new Vector3(tileSet.TileSize * i, 0, tileSet.TileSize * j);
            GameObject go = placeTile(tile, position);
            go.transform.Rotate(Vector3.up, tile.Rotations * 90, Space.Self);
            go.transform.parent = container.transform;
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
                    return Rotations.EndWall(a);
                case WP.Line:
                    return Rotations.LineWall(a);
                case WP.Corner:
                    return Rotations.CornerWall(a);
                case WP.T:
                    return Rotations.TWall(a);
                case WP.X:
                    return Rotations.XWall(a);
                default:
                    return 0;
            }
        }

        private enum Painting
        {
            Wall,
            Floor,
            Empty,
            Door,
            Column,
        }
    }
}