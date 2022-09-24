using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class BuildingSystem : MonoBehaviour
{

    public static BuildingSystem current;

    // GridLayout is an abstract class that is implemented as an interface by Grid
    // Maybe this is used so we don't need to instantiate an instance of a grid?
    public GridLayout gridLayout;

    // Grid component stores data of the layout and helper functions to access data in the grid
    private Grid grid;

    // Tilemap stores sprites within a grid
    [SerializeField] private Tilemap MainTilemap;

    // TileBase is the base class of a tile
    [SerializeField] private TileBase whiteTile;

    public GameObject prefab1;

    private PlaceableObject objectToPlace;

    #region Unity methods

    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
    }


    #endregion

    #region Utils

    public static Vector3 GetMouseWorldPosition()
    {
        // Create a ray from the main camera to a point, in this case, the mouse position.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a raycast and see if it hits a collider. 
        // Ray is just two Vector3Ints which together denote a line or ray.
        // RaycastHit stores information about what the ray hit. We are not passing in raycasthit rather that is being created and is filled in with info from the raycast.
        if(Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            // Return the point that was hit by the ray
            return raycastHit.point;
        }
        else
        {
            // Return a vector3 point to 0,0,0
            return Vector3.zero;
        }
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        // Get the cell position in world space
        Vector3Int cellPos = gridLayout.WorldToCell(position);

        // Get the point at the center of the cell located at cellPos in the world space
        position = grid.GetCellCenterWorld(cellPos);

        // Return the point at the center of the cell
        return position;
    }

    // This method receives an area and tilemap and returns an array of tiles that fall within the area.
    // BoundsInt is an "Axis Aligned Bounding Box" that holds all values as integers
    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {

        // Create an array of tiles. TileBase is the baseclass for the tile object. I guess this is used so that the array can hold other objects that inherit from tilebase
        // The size of the array is = to the size (Volume? No, area since it is 2D) of the area BoundsInt passed in. 
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];

        int counter = 0;

        // Creates a foreach loop that iterates through each positon in a boundsint. 
        foreach (var v in area.allPositionsWithin)
        {

            // Get x and y values from the position but set z to 0
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);

            // Find the tile at the pos created above and save it in the aray of TileBases
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    #endregion

    #region Building Placement

    // Intializes an object for placement
    public void IntializeWithObject(GameObject prefab)
    {
        // Gets the position of the center tile at 0,0 and saves as position
        Vector3 position = SnapCoordinateToGrid(Vector3.zero);

        // Instantiate a new object of the prefab type referenced in the inspector
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);

        // Get the PlaceableObject component from the new object just instantiated.
        // Set that PlaceableObject component = to objectToPlace which was defined at the beginning of the class
        objectToPlace = obj.GetComponent<PlaceableObject>();

        // Add a new ObjectDrag component to the instantiated object
        // This allows the object to be dragged around
        obj.AddComponent<ObjectDrag>();

    }

    // Determines whether the object can be placed by retrieving the area of the placeable object and determining whether any of the tiles in that area are occupied already. 
    private bool CanBePlaced(PlaceableObject placeableObject)
    {
        // Create a bounding box called area
        BoundsInt area = new BoundsInt();

        // Gets the first vertex of the object to place and finds the position on the grid of that vertex. Assigns to area.position
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());

        //TODO Debug
        Debug.Log("area.positon" + area.position);

        // Get size of the object
        area.size = placeableObject.Size;

        // Add 1 to the size of the x and y axes and set as new size. 
        // I think this is to add a buffer
        area.size = new Vector3Int(area.size.x+1, area.size.y+1, area.size.z);

        // Gets an array of tiles based on the modified area of the placeable object
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        // Loop through the array of tiles
        // If the current tile in the passed in area is a while tile, then return false
        // This means that something already exists in that area.
        foreach (var b in baseArray)
        {
            if (b == whiteTile)
            {
                return false;
            }
        }

        // If none of the tiles in the passed in area are white, then the object can be placed
        return true;
    }

    // Fills in area on tilemap with white tiles
    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        // Fills a tilemap with a specified tile from start to end coordinate
        MainTilemap.BoxFill(start, whiteTile, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // If A is pressed, instantiate prefab
            IntializeWithObject(prefab1);
        }

        // If no object to place, do nothing.
        if (!objectToPlace)
        {
            return;
        }

        // If return is pressed, rotate object
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Custom rotate method that not only rotates the object but also resets the Vertex array with rotated vertices.
            objectToPlace.Rotate();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // Determine if object can be placed at that specific place
            if(CanBePlaced(objectToPlace))
            {
                // Place object.
                objectToPlace.Place();

                // Find starting tile in the grid and set as start
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());

                // Fill in the tiles around the object so that other objects can't be placed nearby
                TakeArea(start, objectToPlace.Size);
            }
            else
            {
                // If object can't be placed, destroy it.
                Destroy(objectToPlace.gameObject);
            }
        }
        // If we want to cancel placement, press escape
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(objectToPlace.gameObject);
        }
        
    }
}
