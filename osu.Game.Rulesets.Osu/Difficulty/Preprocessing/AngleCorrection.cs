﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using MathNet.Numerics;
using osu.Game.Rulesets.Osu.Difficulty.Interp;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class AngleCorrection
    {
        public double Evaluate(double distance1, double x, double y)
        {
            double angle = Math.Abs(Math.Atan2(y, x));
            double distance2 = Math.Sqrt(x * x + y * y);

            return SpecialFunctions.Logistic(interp.Evaluate(distance1, distance2, angle));
        }

        /// <summary>
        /// Calculate a correction based on angle - specify a grid of values and interpolate between them
        /// </summary>
        /// <param name="d1">distance between previous note and target note</param>
        /// <param name="d2">other distance (either 2 previous to prev, or target note to next)</param>
        /// <param name="angles">angle between notes</param>
        /// <param name="values">Logistic(values[i,j,k]) is the correction value for  d1[i],d2[j],angle[k]</param>
        public AngleCorrection(double[] d1, double[] d2, double[] angles, double[,,] values)
        {
            interp = new TricubicInterp(d1, d2, angles, values);
        }

        public AngleCorrection(MultiL2NormCorrection correction, double[] d1, double[] d2, double[] angle, string print_name = "", bool print = false)
        {

            double[][][] values = new double[d1.Length][][];
            for (int i = 0; i < d1.Length; ++i)
            {
                values[i] = new double[d2.Length][];
                for (int j = 0; j < d2.Length; ++j)
                {
                    values[i][j] = new double[angle.Length];
                    for (int k = 0; k < angle.Length; ++k)
                    {
                        double x = d2[j] * Math.Cos(angle[k]);
                        double y = d2[j] * Math.Abs(Math.Sin(angle[k]));

                        double distance = d1[i];
                        values[i][j][k] = Math.Clamp(SpecialFunctions.Logit(Math.Clamp(correction.Evaluate(distance, x, y), -0.99999, 0.999999)), -100, 100);
                    }
                }
            }
            if (print_name.Length != 0 && print)
            {
                string indent = "    ";
                string indent2 = indent + indent;
                string indent3 = indent2 + indent;
                string indent4 = indent3 + indent;
                string indent5 = indent4 + indent;

                Console.WriteLine($"{indent2}public static readonly AngleCorrection {print_name} = new AngleCorrection(");

                Console.WriteLine($"{indent3}d1: new double[]{{ {string.Join(", ", d1)} }},");
                Console.WriteLine($"{indent3}d2: new double[]{{ {string.Join(", ", d2)} }},");
                Console.WriteLine($"{indent3}angles: angles,");
                Console.WriteLine($"{indent3}values: new double[,,]{{");

                for (int i=0; i<d1.Length; ++i)
                {
                    Console.WriteLine($"{indent4}{{ // d1={d1[i]}");
                    Console.WriteLine($"{indent5}// 0,   45,   90,   135,   180 degrees ");
                    for (int j = 0; j < d2.Length; ++j)
                    {
                        Console.WriteLine($"{indent4}{indent}{{ {string.Join(", ", values[i][j].Select(x => x.ToString("F2")))} }}, // d2={d2[j]}");
                    }
                    Console.WriteLine($"{indent4}}},");
                }
                Console.WriteLine($"{indent3}}});\n");
            }

            interp = new TricubicInterp(d1, d2, angle, values);
        }

        private TricubicInterp interp;


        private static readonly double[] angles = { 0, 0.25 * Math.PI, 0.5 * Math.PI, 0.75 * Math.PI, Math.PI };

        public static readonly AngleCorrection FLOW_0 = new AngleCorrection(
            d1: new double[] { 0, 0.7, 1, 1.3, 1.7, 3 },
            d2: new double[] { 0, 0.7, 1, 1.3, 1.7, 3 },
            angles: angles,
            values: new double[,,]{
                { // d1=0
                    // 0,   45,   90,   135,   180 degrees
                    { 0.50, 0.50, 0.50, 0.50, 0.50 }, // d2=0
                    { 3.15, 3.15, 3.15, 3.15, 3.15 }, // d2=0.7
                    { 5.47, 5.47, 5.47, 5.47, 5.47 }, // d2=1
                    { 8.18, 8.18, 8.18, 8.18, 8.18 }, // d2=1.3
                    { 12.17, 12.17, 12.17, 12.17, 12.17 }, // d2=1.7
                    { 13.82, 13.82, 13.82, 13.82, 13.82 }, // d2=3
                },
                { // d1=0.7
                    // 0,   45,   90,   135,   180 degrees
                    { -1.38, -1.38, -1.38, -1.38, -1.38 }, // d2=0
                    { 1.26, 0.84, -0.27, -1.12, -1.51 }, // d2=0.7
                    { 2.77, 2.27, 1.00, -0.19, -0.69 }, // d2=1
                    { 4.39, 3.84, 2.45, 1.08, 0.49 }, // d2=1.3
                    { 6.65, 6.05, 4.57, 3.05, 2.41 }, // d2=1.7
                    { 13.82, 13.71, 12.09, 10.43, 9.72 }, // d2=3
                },
                { // d1=1
                    // 0,   45,   90,   135,   180 degrees
                    { -1.80, -1.80, -1.80, -1.80, -1.80 }, // d2=0
                    { -0.26, -0.59, -1.37, -1.82, -2.01 }, // d2=0.7
                    { 0.63, 0.22, -0.89, -1.37, -1.61 }, // d2=1
                    { 1.59, 1.14, 0.03, -0.68, -0.97 }, // d2=1.3
                    { 2.95, 2.47, 1.35, 0.47, 0.14 }, // d2=1.7
                    { 7.60, 7.11, 5.97, 4.92, 4.51 }, // d2=3
                },
                { // d1=1.3
                    // 0,   45,   90,   135,   180 degrees
                    { -1.08, -1.08, -1.08, -1.08, -1.08 }, // d2=0
                    { 0.51, 0.16, -0.73, -1.40, -1.67 }, // d2=0.7
                    { 1.37, 0.91, -0.35, -1.16, -1.52 }, // d2=1
                    { 2.29, 1.77, 0.44, -0.68, -1.12 }, // d2=1.3
                    { 3.56, 3.00, 1.60, 0.26, -0.28 }, // d2=1.7
                    { 7.92, 7.30, 5.78, 4.22, 3.57 }, // d2=3
                },
                { // d1=1.7
                    // 0,   45,   90,   135,   180 degrees
                    { -0.62, -0.62, -0.62, -0.62, -0.62 }, // d2=0
                    { 0.93, 0.56, -0.37, -1.17, -1.49 }, // d2=0.7
                    { 1.73, 1.24, -0.10, -1.16, -1.59 }, // d2=1
                    { 2.56, 2.00, 0.50, -0.97, -1.52 }, // d2=1.3
                    { 3.72, 3.08, 1.42, -0.41, -1.14 }, // d2=1.7
                    { 7.66, 6.91, 4.96, 2.76, 1.71 }, // d2=3
                },
                { // d1=3
                    // 0,   45,   90,   135,   180 degrees
                    { 4.71, 4.71, 4.71, 4.71, 4.71 }, // d2=0
                    { 6.13, 5.76, 4.86, 4.01, 3.68 }, // d2=0.7
                    { 6.82, 6.31, 5.03, 3.83, 3.41 }, // d2=1
                    { 7.54, 6.92, 5.26, 3.74, 3.27 }, // d2=1.3
                    { 8.55, 7.79, 5.69, 3.79, 3.28 }, // d2=1.7
                    { 12.04, 11.04, 8.25, 5.75, 5.07 }, // d2=3
                },
        });

        public static readonly AngleCorrection SNAP_0 = new AngleCorrection(
            d1: new double[] { 0, 1, 2, 3, 5, 9 },
            d2: new double[] { 0, 1, 2, 3, 5, 9 },
            angles: angles,
            values: new double[,,]{
                { // d1=0
                    // 0,   45,   90,   135,   180 degrees
                    { 0.35, 0.35, 0.35, 0.35, 0.35 }, // d2=0
                    { 0.30, 0.41, 0.69, 0.98, 1.10 }, // d2=1
                    { 1.01, 1.15, 1.52, 1.91, 2.07 }, // d2=2
                    { 1.96, 2.12, 2.52, 2.93, 3.09 }, // d2=3
                    { 4.05, 4.22, 4.63, 5.05, 5.23 }, // d2=5
                    { 8.38, 8.55, 8.97, 9.40, 9.57 }, // d2=9
                },
                { // d1=1
                    // 0,   45,   90,   135,   180 degrees
                    { 0.14, 0.14, 0.14, 0.14, 0.14 }, // d2=0
                    { -0.96, -0.63, 0.19, 0.94, 1.19 }, // d2=1
                    { -1.05, -0.57, 0.69, 1.64, 1.85 }, // d2=2
                    { -0.16, 0.35, 1.48, 2.03, 2.11 }, // d2=3
                    { 1.76, 1.95, 2.15, 2.19, 2.19 }, // d2=5
                    { 2.20, 2.20, 2.20, 2.20, 2.20 }, // d2=9
                },
                { // d1=2
                    // 0,   45,   90,   135,   180 degrees
                    { -0.24, -0.24, -0.24, -0.24, -0.24 }, // d2=0
                    { -1.54, -1.18, -0.34, 0.34, 0.53 }, // d2=1
                    { -2.46, -1.78, -0.31, 0.69, 0.84 }, // d2=2
                    { -2.32, -1.50, 0.13, 0.88, 0.94 }, // d2=3
                    { -0.27, 0.28, 0.89, 0.96, 0.97 }, // d2=5
                    { 0.97, 0.97, 0.97, 0.97, 0.97 }, // d2=9
                },
                { // d1=3
                    // 0,   45,   90,   135,   180 degrees
                    { -1.00, -1.00, -1.00, -1.00, -1.00 }, // d2=0
                    { -2.30, -1.93, -1.05, -0.25, 0.02 }, // d2=1
                    { -3.40, -2.69, -1.09, 0.19, 0.45 }, // d2=2
                    { -4.08, -2.97, -0.91, 0.46, 0.61 }, // d2=3
                    { -3.20, -1.92, 0.13, 0.66, 0.68 }, // d2=5
                    { 0.32, 0.57, 0.69, 0.69, 0.69 }, // d2=9
                },
                { // d1=5
                    // 0,   45,   90,   135,   180 degrees
                    { -1.07, -1.07, -1.07, -1.07, -1.07 }, // d2=0
                    { -1.69, -1.50, -1.05, -0.59, -0.40 }, // d2=1
                    { -2.23, -1.84, -0.96, -0.09, 0.27 }, // d2=2
                    { -2.67, -2.05, -0.80, 0.40, 0.85 }, // d2=3
                    { -3.31, -1.93, -0.22, 1.30, 1.67 }, // d2=5
                    { -0.78, 0.10, 1.58, 2.11, 2.16 }, // d2=9
                },
                { // d1=9
                    // 0,   45,   90,   135,   180 degrees
                    { -1.05, -1.05, -1.05, -1.05, -1.05 }, // d2=0
                    { -1.45, -1.33, -1.05, -0.74, -0.61 }, // d2=1
                    { -1.78, -1.56, -1.02, -0.42, -0.13 }, // d2=2
                    { -2.04, -1.71, -0.97, -0.08, 0.35 }, // d2=3
                    { -2.39, -1.73, -0.74, 0.62, 1.19 }, // d2=5
                    { -0.99, -0.61, 0.59, 2.26, 2.92 }, // d2=9
                },
            });

        public static readonly AngleCorrection FLOW_3 = new AngleCorrection(
            d1: new double[] { 0, 0.7, 1, 1.3, 1.7, 3 },
            d2: new double[] { 0, 0.7, 1, 1.3, 1.7, 3 },
            angles: angles,
            values: new double[,,]{
                { // d1=0
                    // 0,   45,   90,   135,   180 degrees
                    { -4.00, -4.00, -4.00, -4.00, -4.00 }, // d2=0
                    { -1.55, -1.55, -1.55, -1.55, -1.55 }, // d2=0.7
                    { -0.50, -0.50, -0.50, -0.50, -0.50 }, // d2=1
                    { 0.55, 0.55, 0.55, 0.55, 0.55 }, // d2=1.3
                    { 1.95, 1.95, 1.95, 1.95, 1.95 }, // d2=1.7
                    { 6.50, 6.50, 6.50, 6.50, 6.50 }, // d2=3
                },
                { // d1=0.7
                    // 0,   45,   90,   135,   180 degrees
                    { -3.53, -3.53, -3.53, -3.53, -3.53 }, // d2=0
                    { -3.12, -2.62, -2.04, -1.52, -1.34 }, // d2=0.7
                    { -2.48, -1.88, -1.18, -0.58, -0.36 }, // d2=1
                    { -1.51, -1.00, -0.24, 0.39, 0.63 }, // d2=1.3
                    { -0.19, 0.25, 1.04, 1.70, 1.96 }, // d2=1.7
                    { 4.13, 4.51, 5.31, 6.02, 6.30 }, // d2=3
                },
                { // d1=1
                    // 0,   45,   90,   135,   180 degrees
                    { -3.26, -3.26, -3.26, -3.26, -3.26 }, // d2=0
                    { -2.89, -2.57, -2.00, -1.43, -1.22 }, // d2=0.7
                    { -2.62, -2.04, -1.31, -0.55, -0.29 }, // d2=1
                    { -2.12, -1.33, -0.48, 0.36, 0.66 }, // d2=1.3
                    { -0.87, -0.23, 0.72, 1.61, 1.94 }, // d2=1.7
                    { 3.30, 3.81, 4.84, 5.79, 6.16 }, // d2=3
                },
                { // d1=1.3
                    // 0,   45,   90,   135,   180 degrees
                    { -2.91, -2.91, -2.91, -2.91, -2.91 }, // d2=0
                    { -2.24, -2.00, -1.51, -1.02, -0.82 }, // d2=0.7
                    { -1.87, -1.46, -0.82, -0.14, 0.12 }, // d2=1
                    { -1.47, -0.81, -0.08, 0.78, 1.08 }, // d2=1.3
                    { -0.45, 0.23, 1.07, 2.04, 2.39 }, // d2=1.7
                    { 3.79, 4.28, 5.28, 6.31, 6.71 }, // d2=3
                },
                { // d1=1.7
                    // 0,   45,   90,   135,   180 degrees
                    { -2.57, -2.57, -2.57, -2.57, -2.57 }, // d2=0
                    { -1.48, -1.32, -0.95, -0.57, -0.41 }, // d2=0.7
                    { -0.96, -0.70, -0.20, 0.34, 0.56 }, // d2=1
                    { -0.40, -0.02, 0.57, 1.28, 1.55 }, // d2=1.3
                    { 0.39, 1.00, 1.65, 2.56, 2.89 }, // d2=1.7
                    { 4.63, 5.03, 5.85, 6.94, 7.35 }, // d2=3
                },
                { // d1=3
                    // 0,   45,   90,   135,   180 degrees
                    { -2.50, -2.50, -2.50, -2.50, -2.50 }, // d2=0
                    { -0.05, -0.05, -0.05, -0.05, -0.05 }, // d2=0.7
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=1
                    { 2.05, 2.05, 2.05, 2.05, 2.05 }, // d2=1.3
                    { 3.45, 3.45, 3.45, 3.45, 3.45 }, // d2=1.7
                    { 8.00, 8.00, 8.00, 8.00, 8.00 }, // d2=3
                },
            });

        public static readonly AngleCorrection SNAP_3 = new AngleCorrection(
            d1: new double[] { 0, 1, 2, 3, 5, 9 },
            d2: new double[] { 0, 1, 2, 3, 5, 9 },
            angles: angles,
            values: new double[,,]{
                { // d1=0
                    // 0,   45,   90,   135,   180 degrees
                    { 0.49, 0.49, 0.49, 0.49, 0.49 }, // d2=0
                    { 1.38, 1.18, 0.68, 0.15, -0.06 }, // d2=1
                    { 2.42, 2.11, 1.29, 0.37, -0.01 }, // d2=2
                    { 3.52, 3.16, 2.20, 1.14, 0.68 }, // d2=3
                    { 5.82, 5.41, 4.36, 3.23, 2.75 }, // d2=5
                    { 10.54, 10.10, 9.00, 7.86, 7.37 }, // d2=9
                },
                { // d1=1
                    // 0,   45,   90,   135,   180 degrees
                    { 0.49, 0.49, 0.49, 0.49, 0.49 }, // d2=0
                    { 1.38, 1.18, 0.68, 0.15, -0.06 }, // d2=1
                    { 2.42, 2.11, 1.29, 0.37, -0.01 }, // d2=2
                    { 3.52, 3.16, 2.20, 1.14, 0.68 }, // d2=3
                    { 5.82, 5.41, 4.36, 3.23, 2.75 }, // d2=5
                    { 10.54, 10.10, 9.00, 7.86, 7.37 }, // d2=9
                },
                { // d1=2
                    // 0,   45,   90,   135,   180 degrees
                    { -0.75, -0.75, -0.75, -0.75, -0.75 }, // d2=0
                    { 0.60, 0.35, -0.22, -0.77, -0.98 }, // d2=1
                    { 1.80, 1.40, 0.45, -0.52, -0.89 }, // d2=2
                    { 2.89, 2.43, 1.30, 0.12, -0.37 }, // d2=3
                    { 5.05, 4.54, 3.30, 2.01, 1.46 }, // d2=5
                    { 9.39, 8.86, 7.56, 6.24, 5.69 }, // d2=9
                },
                { // d1=3
                    // 0,   45,   90,   135,   180 degrees
                    { -2.41, -2.41, -2.41, -2.41, -2.41 }, // d2=0
                    { -0.52, -0.86, -1.59, -2.26, -2.52 }, // d2=1
                    { 1.12, 0.49, -0.77, -1.99, -2.44 }, // d2=2
                    { 2.39, 1.68, 0.13, -1.44, -2.09 }, // d2=3
                    { 4.67, 3.93, 2.18, 0.39, -0.41 }, // d2=5
                    { 9.17, 8.41, 6.57, 4.71, 3.91 }, // d2=9
                },
                { // d1=5
                    // 0,   45,   90,   135,   180 degrees
                    { -2.14, -2.14, -2.14, -2.14, -2.14 }, // d2=0
                    { -0.43, -0.76, -1.52, -2.27, -2.57 }, // d2=1
                    { 1.26, 0.58, -0.84, -2.29, -2.87 }, // d2=2
                    { 2.73, 1.83, -0.06, -2.15, -2.96 }, // d2=3
                    { 5.19, 4.19, 1.83, -0.89, -2.59 }, // d2=5
                    { 9.93, 8.87, 6.22, 3.28, 1.83 }, // d2=9
                },
                { // d1=9
                    // 0,   45,   90,   135,   180 degrees
                    { -2.31, -2.31, -2.31, -2.31, -2.31 }, // d2=0
                    { -0.83, -1.09, -1.70, -2.30, -2.54 }, // d2=1
                    { 0.64, 0.08, -1.08, -2.23, -2.70 }, // d2=2
                    { 2.01, 1.19, -0.41, -2.09, -2.76 }, // d2=3
                    { 4.17, 3.22, 1.14, -1.28, -2.52 }, // d2=5
                    { 8.14, 7.15, 4.72, 1.97, 0.50 }, // d2=9
                },
            });
    }
}
