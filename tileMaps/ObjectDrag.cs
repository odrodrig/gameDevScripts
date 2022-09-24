using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
   private Vector3 offset;

   private void OnMouseDown()
   {
       // Get the difference between where the object was and where the mouse is
       offset = transform.position - BuildingSystem.GetMouseWorldPosition();
   }

   private void OnMouseDrag() {
       
       // Create a new Vector3 position of where the mouse is plus the offset
       Vector3 pos = BuildingSystem.GetMouseWorldPosition() + offset;

       // Move the object to the center of the tile at the position specified
       transform.position = BuildingSystem.current.SnapCoordinateToGrid(pos);
   }
}
