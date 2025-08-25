```

BenchmarkDotNet v0.15.2, macOS Sequoia 15.4.1 (24E263) [Darwin 24.4.0]
Apple M4 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 9.0.5 (9.0.525.21509), Arm64 RyuJIT AdvSIMD
  .NET 9.0 : .NET 9.0.5 (9.0.525.21509), Arm64 RyuJIT AdvSIMD

Job=.NET 9.0  Runtime=.NET 9.0  

```
| Method    | Mean     | Error    | StdDev   | Ratio | Gen0      | Gen1      | Gen2      | Allocated | Alloc Ratio |
|---------- |---------:|---------:|---------:|------:|----------:|----------:|----------:|----------:|------------:|
| Sax       | 14.74 ms | 0.114 ms | 0.107 ms |  0.24 |  843.7500 |  500.0000 |  187.5000 |   5.85 MB |        0.10 |
| XDocument | 62.54 ms | 0.124 ms | 0.189 ms |  1.00 | 8200.0000 | 2800.0000 | 1200.0000 |  59.15 MB |        1.00 |
