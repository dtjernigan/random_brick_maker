using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(int seed, int w_num, int h_num, int w_thres, int w_brick_min, int w_brick_max, int h_brick_min, int h_brick_max, ref object A)
  {
    Random rnd = new Random();

    List<Brep> prim = new List<Brep>();
    List<List<double>> low_pts = new List<List<double>>();

    int x_sum = 0;
    int y_sum = 0;

    for (int i = 0; i < w_num; i++) {
      int width = Width_Height(w_brick_min, w_brick_max, (rnd.Next(1000) * 0.001));
      int height = Width_Height(h_brick_min, h_brick_max, (rnd.Next(1000) * 0.001));
      List<List<double>> none_LowPts = new List<List<double>>();

      add_Prim_add_LowPt(prim, none_LowPts, x_sum, y_sum, x_sum, y_sum, width, height);
      x_sum = x_sum + width;
    }

    for (int a = 0; a < prim.Count; a++) {
      List<double> pt = new List<double>(new double[] { prim[a].Vertices[3].Location.X , prim[a].Vertices[3].Location.Y , 0 });
      low_pts.Add(pt);
    }

    for (int j = 0; j < h_num; j++) {
      int low = lowest_value(low_pts);
      int pprim = 0;
      int nprim = 0;
      int height = Width_Height(h_brick_min, h_brick_max, (rnd.Next(1000) * 0.001));

      for (int l = 0; l < prim.Count; l++) {
        List<double> test = new List<double>(new double[] { prim[l].Vertices[3].Location.X, prim[l].Vertices[3].Location.Y, 0 });
        bool trt = System.Linq.Enumerable.SequenceEqual(low_pts[low], test);
        if (trt) {
          pprim = l;
          List<double> test2 = new List<double>(new double[] { prim[l].Vertices[2].Location.X, prim[l].Vertices[2].Location.Y, 0 });
          for (int o = 0; o < prim.Count; o++) {
            List<double> neigh = new List<double>(new double[] { prim[o].Vertices[3].Location.X, prim[o].Vertices[3].Location.Y, 0 });
            bool trt1 = System.Linq.Enumerable.SequenceEqual(neigh, test2);
            if (trt1) {
              nprim = o;
            }
          }
          break;
        }
      }

      if (nprim > 0) {
        bool found_LowPt = false;
        double neighbor_dist = Math.Abs(prim[pprim].Vertices[0].Location.X - prim[nprim].Vertices[1].Location.X);
        
        List<double> find_Pt = new List<double>(new double[] { prim[nprim].Vertices[3].Location.X, prim[nprim].Vertices[3].Location.Y, 0 });
        for (int q = 0; q < low_pts.Count; q++) {
          found_LowPt = System.Linq.Enumerable.SequenceEqual(find_Pt, low_pts[q]);
          if (found_LowPt) {
            break;
          }
        }

        if (found_LowPt && neighbor_dist > w_thres ) {
          int width = Width_Height(w_brick_min, w_brick_max, (rnd.Next(1000) * 0.001));
          int height1 = Width_Height(h_brick_min, h_brick_max, (rnd.Next(1000) * 0.001));
          int height2 = Width_Height(h_brick_min, h_brick_max, (rnd.Next(1000) * 0.001));

          double val = Math.Abs(prim[pprim].Vertices[3].Location.X - prim[nprim].Vertices[2].Location.X) * (rnd.Next(15, 85) * 0.01);
          double halfx = prim[pprim].Vertices[3].Location.X + val;

          add_Prim_add_LowPt(prim, low_pts, prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y, halfx, prim[pprim].Vertices[2].Location.Y, 0, height1);
          add_Prim_add_LowPt(prim, low_pts, halfx, prim[pprim].Vertices[2].Location.Y, prim[nprim].Vertices[2].Location.X, prim[nprim].Vertices[2].Location.Y, 0, height2);

          remove_LowPt(low_pts, prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y);
          remove_LowPt(low_pts, prim[nprim].Vertices[3].Location.X, prim[nprim].Vertices[3].Location.Y);
        }

        else {
          add_Prim_add_LowPt(prim, low_pts, prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y, prim[pprim].Vertices[2].Location.X, prim[pprim].Vertices[2].Location.Y, 0, height);
          remove_LowPt(low_pts, low_pts[low][0], low_pts[low][1]);
        }
      }
      else {
        add_Prim_add_LowPt(prim, low_pts, prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y, prim[pprim].Vertices[2].Location.X, prim[pprim].Vertices[2].Location.Y, 0, height);
        remove_LowPt(low_pts, low_pts[low][0], low_pts[low][1]);
      }
    }

    A = prim;
  }

  // FUNCTIONS
  public static double clamp(double num, double min_value, double max_value) {
    num = Math.Max(Math.Min(num, max_value), min_value);
    return num;
  }

  public static int lowest_value(List<List<double>> array_pts) {
    var a_size = array_pts.Count;
    var temp_pos = array_pts[0];
    var id = 0;
    for (int k = 1; k < a_size; k++) {
      if (array_pts[k][1] < temp_pos[1]) {
        temp_pos = array_pts[k];
        id = k;
      }
    }
    return id;
  }

  public static int Width_Height(int brick_Min, int brick_Max, double factor) {
    int WH_Calc = Convert.ToInt32(Math.Ceiling(clamp((factor) *brick_Max, brick_Min, brick_Max)));
    if (WH_Calc % 2 > 0){
      WH_Calc += 1;
    }
    return WH_Calc;
  }

  public static void add_Prim_add_LowPt(List<Brep> prim_List, List<List<double>> low_pts_List, double pos_xx, double pos_xy, double pos_yx, double pos_yy, int width_Add, int height_Add) {
    Point3d p3d_xx = new Point3d(pos_xx, pos_xy, 0);
    Point3d p3d_xy = new Point3d(pos_yx + width_Add, pos_yy, 0);
    Point3d p3d_yx = new Point3d(pos_yx + width_Add, pos_yy + height_Add, 0);
    Point3d p3d_yy = new Point3d(pos_xx, pos_yy + height_Add, 0);

    prim_List.Add(Rhino.Geometry.Brep.CreateFromCornerPoints(p3d_xx, p3d_xy, p3d_yx, p3d_yy, 0.01));
    if (low_pts_List.Count > 0){
      List<double> new_Low_pt = new List<double>(new double[] {pos_xx, pos_yy + height_Add, 0});
      low_pts_List.Add(new_Low_pt);
    }
  }

  public static void remove_LowPt(List<List<double>> low_pts_List, double pos_xx, double pos_xy) {
    List<double> remove_Pt = new List<double>(new double[] {pos_xx, pos_xy, 0});
    for (int s = 0; s < low_pts_List.Count; s++) {
      bool bool_test = System.Linq.Enumerable.SequenceEqual(low_pts_List[s], remove_Pt);
      if ( bool_test ){
        low_pts_List.RemoveAt(s);
      }
    }
  }
  // FUNCTIONS
}
