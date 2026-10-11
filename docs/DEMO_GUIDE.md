# Hướng dẫn trình diễn

Trình tự trình diễn tham khảo trong 5–7 phút; thời lượng có thể điều chỉnh theo yêu cầu môn học.

| Thời điểm gợi ý | Thao tác trên màn hình | Ý chính cần giải thích |
| --- | --- | --- |
| 0:00–0:40 | Mở solution, chạy project WinForms | Desktop, một tiến trình, cold start, page/frame |
| 0:40–1:40 | Mở textbook.csv, 3 frame, FIFO; bước từng trang | Hit không đổi FIFO queue; fault khác replacement |
| 1:40–2:30 | Tính so sánh 4 thuật toán | FIFO 15, LRU 12, OPT 9, Clock 14; OPT biết tương lai |
| 2:30–3:30 | Mở sc_clock.csv, Second-Chance | Bước 5 xóa R toàn vòng, bước 7 giữ page 2 và loại page 3 |
| 3:30–4:20 | Belady; curve tới 5 | Thêm frame không luôn giảm fault với FIFO |
| 4:20–5:10 | Xuất CSV, mở summary/trace; thử input lỗi | Tái lập, kiểm tra dữ liệu, không crash |
| 5:10–6:00 | Chạy test_windows.bat; sau benchmark mở performance.csv | Expected/actual, stress, seed, đo thật; trình bày log C# đã chạy thực tế |
| 6:00–7:00 | Giới thiệu repo, phân công và giới hạn | Phân công thực hiện; giới hạn mô hình TLB/dirty/I/O |

Trước quay: build/test bản C# và sinh evidence/benchmark; điền thông tin nhóm, chạy nghiệm thu Windows, tăng cỡ chữ Terminal, tắt thông báo riêng tư và dùng dữ liệu mẫu. Sau đăng: thử link khi không đăng nhập và điền URL vào báo cáo/README. Không đọc toàn bộ mã nguồn trong video; chọn một ca kiểm thử và giải thích quyết định chọn victim.
