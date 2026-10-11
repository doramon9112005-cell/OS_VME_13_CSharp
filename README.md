# OS_VME_13 — C# Windows Forms

Project **C# / .NET 8 / Windows Forms** mô phỏng FIFO, LRU, OPT và Second-Chance (Clock). Toàn bộ mã ứng dụng, CLI, kiểm thử và benchmark đều viết bằng C#. Không cần Python; không có gói NuGet bên thứ ba.

## Mở và chạy trên Windows

**Cách 1 — Visual Studio**

1. Cài Visual Studio 2022 hỗ trợ .NET 8 (17.8 trở lên), chọn workload **.NET desktop development** và .NET 8 SDK.
2. Giải nén ZIP, mở `OS_VME_13.sln`.
3. Nhấp phải project **PageReplacement.WinForms** → **Set as Startup Project**.
4. Nhấn **F5** hoặc **Ctrl+F5**. Giao diện được dựng bằng mã C# trong `MainForm.cs`, không dùng Designer/resx.

**Cách 2 — .NET CLI**

Cài [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), rồi nhấp đúp `run_windows.bat`. Hoặc mở terminal ở thư mục chứa solution:

```powershell
dotnet build OS_VME_13.sln -c Release
dotnet run --project source/PageReplacement.WinForms -c Release
```

GUI chỉ chạy trên Windows. Thư viện Core, CLI và test runner target `net8.0` và được thiết kế chạy đa nền tảng. `EnableWindowsTargeting` chỉ cho phép build dự án Windows từ máy khác; không làm WinForms chạy được trên Linux. Restore/build lần đầu có thể cần internet để lấy targeting pack.

## Trạng thái kiểm chứng quan trọng

**Cập nhật từ log người dùng ngày 10/10/2026:** test Core/CSV đã chạy **25/25 PASS** trên Windows. Build WinForms báo CS0108 vì hàm `Layout` trùng tên sự kiện `Control.Layout`; bản cập nhật đã đổi tên hàm và cả bốn chỗ gọi thành `CreateTableLayout`. Chưa có kết quả build/GUI sau sửa. Trạng thái chưa chạy trong báo cáo Word phản ánh thời điểm bàn giao ban đầu; dùng cập nhật này làm trạng thái mới nhất.

Mã C# đã được rà soát và phân tích cú pháp, nhưng **chưa biên dịch/chưa chạy test C# trong môi trường bàn giao**, vì CoreCLR không khởi động (`0x8007000E`). Không coi mã đã viết là đã PASS. Không chuyển log hoặc thời gian benchmark Python cũ sang bản C#.

`output/VALIDATION_STATUS.md` ghi trạng thái. Các tệp actual/performance sẽ được tạo bằng lệnh dưới đây trên máy bạn. Trước nộp cần build thành công, chạy test và nghiệm thu GUI theo `docs/WINDOWS_ACCEPTANCE.md`.

## Các chức năng

- Input tham chiếu gõ tay hoặc mở CSV; số frame và thuật toán tùy chọn.
- Chạy từng bước, đầu/cuối, chọn dòng, tự chạy/dừng, khoảng 100–2000 ms/bước.
- Frame giữ vị trí vật lý; hiển thị hit/fault, victim, R, hand và các vị trí bị xóa R.
- So sánh bốn thuật toán và vẽ đường cong số lỗi theo frame.
- Mẫu slide, mẫu Belady; xuất trace, summary, curve và input tái lập.
- GUI tính ở `Task.Run`, chỉ cập nhật control khi quay lại UI thread sau `await`.

Màu xanh là hit, cam là fault; `>` đánh dấu Clock hand. Bit R của page mới bằng 1, hit không dịch hand; không reset R định kỳ. Nếu sửa input sau khi chạy, bấm Mô phỏng/Tính so sánh để cập nhật; xuất file dùng kết quả đã tính. Xuất so sánh hỏi xác nhận nếu trùng tên file.

## CSV input

UTF-8, chấp nhận BOM, header chính xác, một file tương ứng một ca:

```csv
step,page,frames
1,7,3
2,0,3
3,1,3
```

| Cột | Quy tắc |
| --- | --- |
| step | Số nguyên liên tục từ 1, dòng là thứ tự truy cập |
| page | Số nguyên 0..2147483647; page 0 hợp lệ |
| frames | Số nguyên dương, giống nhau trên mọi dòng |

GUI: 1..5000 tham chiếu, 1..32 frame. Core/CLI: 1..200000 tham chiếu, 1..256 frame. Trace có giới hạn n×f ≤ 1000000; dùng `--no-trace` với dữ liệu lớn. Input CSV cho phép trường quoted nhưng không có trường nhiều dòng (các giá trị đều là số). Từ chối header sai, dòng trắng trong dữ liệu, thiếu/thừa cột, số âm/thập phân/tràn Int32, step sai hoặc frame thay đổi. Nhập tay chấp nhận dấu phẩy/khoảng trắng.

## CSV output

Trace ghi trạng thái **sau** từng truy cập. `step` bắt đầu từ 1; `slot`, `hand_after` và `cleared_slots_json` bắt đầu từ 0. `victim` rỗng khi không loại trang; 0 là victim hợp lệ. `frames_json` là mảng JSON có `null` tại frame trống. `reference_bits_json`, `hand_after` chỉ dùng cho Clock. `order_pages_json`: FIFO từ cũ tới mới; LRU từ ít tới nhiều được dùng gần đây. Fault rate ở summary là tỷ lệ 0..1. Khác với fault, replacement chỉ tăng khi frame đầy và thực sự có victim.

## Kiểm thử và sinh bằng chứng bằng C#

```powershell
# Test runner C# tự chứa, không dùng xUnit/NUnit, không chạy qua dotnet test
dotnet run --project tests/PageReplacement.Tests -c Release -- .

# Sinh expected/actual, summary, trace và đường cong từ bản C#
dotnet run --project source/PageReplacement.Cli -c Release -- evidence .

# Ví dụ chạy CLI, kiểm chứng thêm luồng nhập/xuất
dotnet run --project source/PageReplacement.Cli -c Release -- simulate input/textbook.csv --output output/my_run --curve 10

# Smoke test GUI thực, chỉ trên Windows
dotnet run --project source/PageReplacement.WinForms -c Release -- --smoke
```

`test_windows.bat` chạy test core/CSV rồi smoke test WinForms và dừng nếu lỗi. Runner trả exit 1 khi assertion không đạt, CLI trả exit 2 với input không hợp lệ. Test runner ghi `output/test_results.csv` và `test_environment.json`; GUI smoke ghi `gui_smoke.txt` cùng ảnh thật `gui_smoke.png` nếu thành công. Có 25 nhóm test, bao gồm 100 chuỗi ngẫu nhiên so oracle độc lập, 3276 cặp input/frame vét cạn OPT, stress 200000 lần truy cập cả khi hit nhiều và miss hoàn toàn. Đây là phạm vi test đã viết, chưa phải kết quả đã chạy.

Mốc expected trong slide ch10: chuỗi 22 tham chiếu, f=3 → **FIFO 15, LRU 12, OPT 9**. Clock theo quy ước project expected **14**. Belady với FIFO: f=3 → 9, f=4 → 10. Các expected là oracle, không phải bằng chứng C# đã đạt.

## Benchmark

Nhấp đúp `benchmark_windows.bat` hoặc dùng PowerShell:

```powershell
$env:DOTNET_TieredCompilation='0'
dotnet run --project source/PageReplacement.Cli -c Release -- benchmark output
```

Benchmark dùng LCG32 seed13 (mã generator đi kèm), ba workload Uniform/Locality/Cyclic scan; n=1000/10000/100000; f=16. Ba warmup và năm lần đo, ghi median/min/max. `Stopwatch` đo thời gian; lượt riêng dùng `GC.GetAllocatedBytesForCurrentThread` đo **tổng byte được cấp phát**, không phải peak RAM/RSS. Input, GUI và CSV không nằm trong phép đo; validation/tiền xử lý OPT có nằm trong phép đo. `trace_memory.csv` so tổng cấp phát trace bật/tắt. Mã hash input và môi trường .NET được lưu để tái lập.

Sau chạy, đưa số đo C# vào phần 7–8 của báo cáo và điền trạng thái kiểm thử. Không sử dụng benchmark Python cũ như số đo C#.

## Tạo EXE

Nhấp đúp `publish_windows.bat` để publish **Windows x64 self-contained**. EXE được tạo tại `publish/win-x64/PageReplacement.exe`, dự kiến không cần cài .NET Runtime trên máy nhận. Bước này cần SDK và có thể cần mạng để tải runtime pack. **Gói bàn giao chưa có EXE đã build**, chưa được xác nhận khả năng chạy trên máy đích.

## Cấu trúc solution

| Project / thư mục | Vai trò |
| --- | --- |
| source/PageReplacement.Core | Simulator.cs, mô hình kết quả, CsvFiles.cs |
| source/PageReplacement.WinForms | MainForm.cs, CurveView.cs, Program.cs |
| source/PageReplacement.Cli | Lệnh simulate/evidence/benchmark |
| tests/PageReplacement.Tests | Test runner C#, oracle độc lập, stress |
| input | CSV thường, biên và dữ liệu lỗi có chủ đích |
| output | Trạng thái xác minh; kết quả C# sinh sau khi chạy |
| report | Báo cáo Word C# |
| docs | Pseudocode, sơ đồ, Q1–Q6, checklist, nghiệm thu, phân công, demo |

## Git và các bước còn lại

Bản Source Clean chỉ chứa mã nguồn, dữ liệu mẫu và tài liệu. Không có thư mục .git, file .bundle, cấu hình .github hay lịch sử commit đi kèm. Giải nén rồi đưa các tệp bên trong thư mục OS_VME_13_CSharp lên repository của bạn. Các thư mục bin/obj/.vs sinh ra khi chạy trên máy cũng không cần đưa lên repository. Repo từ xa và video chưa được đăng.

Trước nộp: build/test trên Windows; ghi số đo C#; điền thành viên, MSSV, lớp, giảng viên, đóng góp thật; thêm link repo/video. Bạn tự quay video; `docs/DEMO_GUIDE.md` có trình tự gợi ý. AI hỗ trợ tạo bản đầu, nhóm phải tự chạy lại và giải thích được mã nguồn.
