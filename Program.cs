using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Collision
{
    class Program
    {
        static void Main(string[] args)
        {
            var spheres = new Sphere[50000];
            using (var f = new StreamReader("d:\\spheres.dat"))
            {
                for (var i = 0; i < 50000; i++)
                {
                    float x = float.Parse(f.ReadLine(), CultureInfo.InvariantCulture);
                    float y = float.Parse(f.ReadLine(), CultureInfo.InvariantCulture);
                    float z = float.Parse(f.ReadLine(), CultureInfo.InvariantCulture);
                    float r = float.Parse(f.ReadLine(), CultureInfo.InvariantCulture);
                    spheres[i] = new Sphere(new Vec3(x, y, z), r);
                }
            }

            var before0 = GC.CollectionCount(0);
            var before1 = GC.CollectionCount(1);
            var before2 = GC.CollectionCount(2);

            var sw = Stopwatch.StartNew();

            run(spheres);

            sw.Stop();

            Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Gen 0: {GC.CollectionCount(0) - before0}");
            Console.WriteLine($"Gen 1: {GC.CollectionCount(1) - before1}");
            Console.WriteLine($"Gen 2: {GC.CollectionCount(2) - before2}");
            Console.WriteLine($"Mem: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} mb");
        }

        static void run(Sphere[] spheres)
        {
            var iMax = spheres.Length;
            Parallel.For(0, iMax, i =>
            {
                for (var j = i + 1; j < iMax; j++)
                    if (spheres[i].CollidesWith(spheres[j]))
                        Console.WriteLine("{0} collides with {1}", i, j);
            });

            //for (var i = 0; i < iMax; i++)
            //    for (var j = i + 1; j < iMax; j++)
            //        if (spheres[i].CollidesWith(spheres[j]))
            //            Console.WriteLine("{0} collides with {1}", i, j);
        }

        private readonly struct Vec3
        {
            private readonly float x;
            private readonly float y;
            private readonly float z;

            public Vec3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
            public readonly float LenSq() => (x * x) + (y * y) + (z * z);
        }

        private readonly struct Sphere
        {
            private readonly Vec3 pos;
            private readonly float r;

            public Sphere(Vec3 pos, float r)
            {
                this.pos = pos;
                this.r = r;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool CollidesWith(in Sphere other)
            {
                var radSum = this.r + other.r;
                return (other.pos - this.pos).LenSq() < radSum * radSum;
            }
        }
    }
}