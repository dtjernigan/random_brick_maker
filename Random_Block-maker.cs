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
  private void RunScript(int seed, int w_num, int h_num, int w_thres, int w_brick_min, int w_brick_max, int h_brick_min, int h_brick_max, ref object A, ref object B)
  {
    Random rnd = new Random();

    List<Brep> prim = new List<Brep>();
    List<List<double>> low_pts = new List<List<double>>();

    int x_sum = 0;
    int y_sum = 0;
    int method1 = 0;

    for (int i = 0; i < w_num; i++) {
      int width = Convert.ToInt16(Math.Ceiling(clamp((rnd.Next(1000) * 0.001) * w_brick_max, w_brick_min, w_brick_max)));
      if (width % 2 > 0){
        width += 1;
      }
      int height = Convert.ToInt16(Math.Ceiling(clamp((rnd.Next(1000) * 0.001) * h_brick_max, h_brick_min, h_brick_max)));
      if (height % 2 > 0){
        height += 1;
      }

      Point3d pt0 = new Point3d(x_sum, y_sum, 0);
      Point3d pt1 = new Point3d(x_sum + width, y_sum, 0);
      Point3d pt2 = new Point3d(x_sum + width, y_sum + height, 0);
      Point3d pt3 = new Point3d(x_sum, y_sum + height, 0);

      x_sum = x_sum + width;

      prim.Add(Rhino.Geometry.Brep.CreateFromCornerPoints(pt0, pt1, pt2, pt3, 0.01));
    }

    for (int a = 0; a < prim.Count; a++) {
      List<double> pt = new List<double>(new double[] { prim[a].Vertices[3].Location.X , prim[a].Vertices[3].Location.Y , 0 });
      low_pts.Add(pt);
    }

    for (int j = 0; j < h_num; j++) {
      int low = lowest_value(low_pts);
      int pprim = 0;
      int nprim = 0;

      int height = Convert.ToInt16(Math.Ceiling(clamp((rnd.Next(1000) * 0.001) * h_brick_max, h_brick_min, h_brick_max)));
      if (height % 2 > 0){
        height += 1;
      }

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
        List<double> ptt = new List<double>(new double[] { prim[nprim].Vertices[3].Location.X, prim[nprim].Vertices[3].Location.Y, 0 });
        int cnt = 0;
        for (int q = 0; q < low_pts.Count; q++) {
          bool trt6 = System.Linq.Enumerable.SequenceEqual(ptt, low_pts[q]);
          if ( trt6 ){
            cnt += 1;
          }
        }

        double testt = Math.Abs(prim[pprim].Vertices[0].Location.X - prim[nprim].Vertices[1].Location.X);
        if (testt > w_thres && cnt > 0) {
          //Print("Method 1");
          method1 += 1;

          int height1 = Convert.ToInt16(Math.Ceiling(clamp((rnd.Next(1000) * 0.001) * h_brick_max, h_brick_min, h_brick_max)));
          if (height1 % 2 > 0){
            height1 += 1;
          }
          int height2 = Convert.ToInt16(Math.Ceiling(clamp((rnd.Next(1000) * 0.001) * h_brick_max, h_brick_min, h_brick_max)));
          if (height2 % 2 > 0){
            height2 += 1;
          }

          double val = Math.Abs(prim[pprim].Vertices[3].Location.X - prim[nprim].Vertices[2].Location.X) * (rnd.Next(15, 85) * 0.01);
          double halfx = prim[pprim].Vertices[3].Location.X + val;

          List<double> p10 = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y , 0});
          List<double> p11 = new List<double>(new double[] {halfx , prim[pprim].Vertices[2].Location.Y , 0});
          List<double> p12 = new List<double>(new double[] {halfx, prim[pprim].Vertices[2].Location.Y + height1, 0});
          List<double> p13 = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[2].Location.Y + height1 , 0});

          List<double> p20 = new List<double>(new double[] {halfx , prim[pprim].Vertices[2].Location.Y , 0});
          List<double> p21 = new List<double>(new double[] {prim[nprim].Vertices[2].Location.X, prim[nprim].Vertices[2].Location.Y , 0});
          List<double> p22 = new List<double>(new double[] {prim[nprim].Vertices[2].Location.X, prim[nprim].Vertices[2].Location.Y + height2 , 0});
          List<double> p23 = new List<double>(new double[] {halfx, prim[pprim].Vertices[2].Location.Y + height2 , 0});

          Point3d pt10 = new Point3d(p10[0], p10[1], 0);
          Point3d pt11 = new Point3d(p11[0], p11[1], 0);
          Point3d pt12 = new Point3d(p12[0], p12[1], 0);
          Point3d pt13 = new Point3d(p13[0], p13[1], 0);

          Point3d pt20 = new Point3d(p20[0], p20[1], 0);
          Point3d pt21 = new Point3d(p21[0], p21[1], 0);
          Point3d pt22 = new Point3d(p22[0], p22[1], 0);
          Point3d pt23 = new Point3d(p23[0], p23[1], 0);

          prim.Add(Rhino.Geometry.Brep.CreateFromCornerPoints(pt10, pt11, pt12, pt13, 0.01));
          prim.Add(Rhino.Geometry.Brep.CreateFromCornerPoints(pt20, pt21, pt22, pt23, 0.01));

          low_pts.Add(p13);
          low_pts.Add(p23);

          List<double> pttt = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y, 0});
          for (int s = 0; s < low_pts.Count; s++) {
            bool trt2 = System.Linq.Enumerable.SequenceEqual(low_pts[s], pttt);
            if ( trt2 ){
              low_pts.RemoveAt(s);
              //Print("Remove 1 - 1");
            }
          }

          List<double> ptttt = new List<double>(new double[] {prim[nprim].Vertices[3].Location.X, prim[nprim].Vertices[3].Location.Y, 0});
          for (int t = 0; t < low_pts.Count; t++) {
            bool trt3 = System.Linq.Enumerable.SequenceEqual(low_pts[t], ptttt);
            if ( trt3 ){
              low_pts.RemoveAt(t);
              //Print("Remove 1 - 2");
            }
          }
        }
        else {
          //Print("Method 2");
          List<double> p0 = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y, 0});
          List<double> p1 = new List<double>(new double[] {prim[pprim].Vertices[2].Location.X, prim[pprim].Vertices[2].Location.Y, 0});
          List<double> p2 = new List<double>(new double[] {prim[pprim].Vertices[2].Location.X, prim[pprim].Vertices[2].Location.Y + height, 0});
          List<double> p3 = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[2].Location.Y + height, 0});

          Point3d pt0 = new Point3d(p0[0], p0[1], 0);
          Point3d pt1 = new Point3d(p1[0], p1[1], 0);
          Point3d pt2 = new Point3d(p2[0], p2[1], 0);
          Point3d pt3 = new Point3d(p3[0], p3[1], 0);

          prim.Add(Rhino.Geometry.Brep.CreateFromCornerPoints(pt0, pt1, pt2, pt3, 0.01));

          for (int s = 0; s < low_pts.Count; s++) {
            bool trt4 = System.Linq.Enumerable.SequenceEqual(low_pts[s], low_pts[low]);
            if ( trt4 ){
              low_pts.RemoveAt(s);
              //Print("Remove 2");
            }
          }

          low_pts.Add(p3);
        }
      }
      else {
        //Print("Method 3");
        List<double> p0 = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[3].Location.Y, 0});
        List<double> p1 = new List<double>(new double[] {prim[pprim].Vertices[2].Location.X, prim[pprim].Vertices[2].Location.Y, 0});
        List<double> p2 = new List<double>(new double[] {prim[pprim].Vertices[2].Location.X, prim[pprim].Vertices[2].Location.Y + height, 0});
        List<double> p3 = new List<double>(new double[] {prim[pprim].Vertices[3].Location.X, prim[pprim].Vertices[2].Location.Y + height, 0});

        Point3d pt0 = new Point3d(p0[0], p0[1], 0);
        Point3d pt1 = new Point3d(p1[0], p1[1], 0);
        Point3d pt2 = new Point3d(p2[0], p2[1], 0);
        Point3d pt3 = new Point3d(p3[0], p3[1], 0);

        prim.Add(Rhino.Geometry.Brep.CreateFromCornerPoints(pt0, pt1, pt2, pt3, 0.01));

        for (int s = 0; s < low_pts.Count; s++) {
          bool trt5 = System.Linq.Enumerable.SequenceEqual(low_pts[s], low_pts[low]);
          if ( trt5 ){
            low_pts.RemoveAt(s);
          }
        }

        low_pts.Add(p3);
      }
    }


    A = prim;
    B = method1;

  }

  // <Custom additional code> 
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
  // </Custom additional code> 
}