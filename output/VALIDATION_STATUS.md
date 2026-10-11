# Trạng thái xác minh bản C#

## Cập nhật theo log người dùng ngày 10/10/2026

- Core/CSV: **25/25 PASS** theo log console người dùng gửi; bao gồm kiểm thử oracle, vét cạn OPT và stress.
- WinForms: build dừng với CS0108 tại hàm `MainForm.Layout(int, int)`.
- Đã sửa bằng cách đổi tên hàm và bốn chỗ gọi thành `CreateTableLayout`; giữ `TreatWarningsAsErrors=true`.
- Chưa nhận kết quả build/smoke test GUI sau sửa; chưa có benchmark.
- Đây là kết quả người dùng cung cấp, không phải test chạy trong môi trường tạo project.

## Trạng thái lúc bàn giao ban đầu

- Mã nguồn đã được rà soát và đối chiếu với quy ước thuật toán/CSV.
- .NET SDK 8.0.425 tải từ Microsoft, kiểm tra SHA-512 thành công.
- Host nhận SDK nhưng CoreCLR không khởi động trong môi trường tạo project: `Failed to create CoreCLR, HRESULT: 0x8007000E`.
- Chưa biên dịch C#, chưa chạy test runner C#, chưa đo benchmark C#, chưa chạy GUI Windows.
- Không có tệp PASS/actual/performance giả; output được sinh khi bạn chạy các lệnh trong README.
- Test runner C# gồm 25 nhóm test, oracle độc lập và 3276 trường hợp vét cạn OPT; đó là phạm vi đã viết, chưa phải kết quả đã thực thi.
- Cấu hình GitHub Actions đi kèm nhưng chưa chạy từ xa.
