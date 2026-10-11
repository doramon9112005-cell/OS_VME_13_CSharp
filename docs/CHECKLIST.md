# Checklist OS_VME_13 bản C#

| Hạng mục | Trạng thái |
| --- | --- |
| FIFO, LRU, OPT, Clock | Core đạt 25/25 PASS theo log kiểm thử Windows |
| Desktop Windows GUI | Đã viết WinForms, chưa nghiệm thu runtime |
| Core/GUI/CSV tách biệt | Có 4 project trong solution |
| Nhập/xuất CSV, lỗi dữ liệu | Đã viết parser/writer và test |
| Frame, R, hand, chạy từng bước | Có MainForm.cs và TraceStep |
| So sánh và Belady | Có CurveView và dữ liệu expected |
| Test thường/biên/tie/stress | 25/25 nhóm test PASS theo log ngày 10/10/2026 |
| OPT oracle độc lập | 3276 trường hợp vét cạn trong test runner |
| Hiệu năng | Có benchmark C#; chưa có số đo C# |
| Báo cáo, pseudocode, Q1–Q6 | Có tài liệu cập nhật C#; phần actual/số đo chờ chạy |
| Build Windows | Có lệnh và batch; GUI sau sửa CS0108 chờ xác nhận |
| Git repository | Source không kèm kho Git; URL repository chờ bổ sung |
| Thành viên, đóng góp, tiến độ thật | Cập nhật theo kết quả thực hiện |
| Video | Quay trình diễn chức năng và bổ sung liên kết |

Đối chiếu chức năng với đề tài OS_VME_13 và yêu cầu chung của môn học. Trạng thái nghiệm thu GUI được ghi riêng trong docs/WINDOWS_ACCEPTANCE.md.
