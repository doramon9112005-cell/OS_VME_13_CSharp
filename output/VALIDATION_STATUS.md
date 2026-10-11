# Trạng thái kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Core và CSV | 25/25 nhóm test PASS theo log console chạy trên Windows ngày 10/10/2026 |
| Phạm vi kiểm thử | Oracle slide, Clock, Belady, 100 chuỗi ngẫu nhiên, 3276 ca vét cạn OPT, stress 200000 tham chiếu, CSV và snapshot |
| WinForms | Đã đổi Layout thành CreateTableLayout để sửa CS0108; chưa xác nhận build và smoke test sau sửa |
| Benchmark | Chưa có số đo thực thi |
| Phân tích cú pháp | 8 tệp C# không có lỗi cú pháp theo bộ phân tích; không thay thế kiểm tra biên dịch |

## Tái lập

1. Chạy `test_windows.bat` để kiểm thử Core/CSV và smoke test WinForms.
2. Chạy lệnh `evidence` trong README để tạo bảng expected/actual và trace.
3. Chạy `benchmark_windows.bat` để đo hiệu năng.
4. Hoàn thành `docs/WINDOWS_ACCEPTANCE.md` để nghiệm thu các thao tác GUI.

Log được cung cấp xác nhận Core/CSV; chưa có tệp kết quả máy chạy kèm theo. Các tệp CSV/JSON kết quả sẽ được tạo bởi các lệnh trên. Báo cáo Word hiện phản ánh trạng thái kiểm chứng tại thời điểm soạn ban đầu; cần cập nhật phần kết quả theo lần chạy nghiệm thu cuối.
