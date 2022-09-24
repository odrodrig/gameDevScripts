using System.Collections;
using static System.Math;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
   public bool Placed { get; private set;}
   public Vector3Int Size { get; private set;}
   private Vector3[] Vertices;

   private void GetColliderVertexPositionsLocal()
   {
       // Get the BoxCollider object and store it as b
       BoxCollider b = gameObject.GetComponent<BoxCollider>();

       // Create an array of Vector3s size of 4
       Vertices = new Vector3[4];

       // Find the veticies that touch the grid by starting at the center of the box colider then adding half of each size based on axis
       // We don't care about the vertices on top not touching the grid. Basically the vertices of one face
       Vertices[0] = b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
       Vertices[1] = b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;
       Vertices[2] = b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
       Vertices[3] = b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
   }

   private void CalculateSizeInCells()
   {
        // Create an array of vertices equal to the colider vertex positions
        Vector3Int[] vertices = new Vector3Int[Vertices.Length];

        // Loop through our new vertices array
        for (int i = 0; i < vertices.Length; i++)
        {
            // Get's the vertex point from the array and finds the position of that vertex in the world.
            Debug.Log("In CalculateSize " + Vertices[i]);
            Vector3 worldPos = transform.TransformPoint(Vertices[i]);
            Debug.Log("WorldPos: " + worldPos);

            // Finds the local/cell position of the vertex and saves it in vertices array
            vertices[i] = BuildingSystem.current.gridLayout.WorldToCell(worldPos);
        }

        // TODO: Debug
        Debug.Log("vetices");
        Debug.Log(vertices);

        // Find the size of the box collider on the grid
        // Subtract vertices to find the distance between them.
        // Use absolute value so we get a positive value.
        // We don't care about z on our grid. Set to 1, not 0.
        Size = new Vector3Int(System.Math.Abs((vertices[0] - vertices[1]).x),
                                System.Math.Abs((vertices[0] - vertices[3]).y),
                                1);

        Debug.Log("size: " + Size);
       
   }

   // Returns position of the first vertex of the box collider
   public Vector3 GetStartPosition()
   {
       // Get world space from local space
       return transform.TransformPoint(Vertices[0]);
   }

    // When we start automatically get location of all vertices and the tile size of the object
   private void Start()
   {
       GetColliderVertexPositionsLocal();
       CalculateSizeInCells();
   }

   public void Rotate()
   {
       // Rotate the object 90 degrees
       transform.Rotate(new Vector3(0,90,0));
       
       // Change the size by flipping x and y values
       Size = new Vector3Int(Size.y, Size.x, 1);

       Vector3[] vertices = new Vector3[Vertices.Length];

       // Loop that goes through each vertex and shifts it one up in the array. Sets the vertex that was at 4 back to 0.
       for (int i = 0; i < vertices.Length; i++)
       {
           vertices[i] = Vertices[(i + 1) % Vertices.Length];
       }

       Vertices = vertices;
   }

   public virtual void Place()
   {
       // Get the ObjectDrag component
       ObjectDrag drag = gameObject.GetComponent<ObjectDrag>();
       
       // Destroy the objectDrag component. 
       // This prevents the object from being dragged further
       Destroy(drag);

       // Set Placed to true. You can implement logic to stop to do something once placed or something
       Placed = true;

   }


}
