# Nghiệm thu Windows bản C#

Ghi ngày, phiên bản Windows/.NET, máy và người thử. Actual ban đầu của mọi dòng: **chưa chạy**.

| ID | Thao tác | Expected |
| --- | --- | --- |
| W01 | dotnet build OS_VME_13.sln -c Release | Không lỗi biên dịch |
| W02 | test_windows.bat | Core/CSV test PASS; GUI smoke PASS, có ảnh thật |
| W03 | run_windows.bat hoặc F5 WinForms | Mở desktop, chữ Việt và bố cục rõ |
| W04 | Mẫu slide, f=3, lần lượt bốn policy | FIFO=15, LRU=12, OPT=9, Clock=14 faults |
| W05 | Đầu/Sau/Trước/Cuối, chọn dòng | Frame và counters đồng bộ, bước 0 trống |
| W06 | Tự chạy/dừng, tốc độ, đóng khi tính | Không treo, không cập nhật control đã dispose |
| W07 | sc_clock.csv, Clock | Bước 5 xóa R tại 0/1/2; bước 7 victim=3 |
| W08 | Belady, curve tới 5 | FIFO f=3 là 9, f=4 là 10 |
| W09 | Xuất trace và so sánh | CSV parse đúng, victim 0 không mất, JSON đủ |
| W10 | Input rỗng, âm, tràn Int32, CSV sai | Thông báo lỗi và tiếp tục dùng được |
| W11 | Hủy dialog; thư mục không có quyền | Không crash; lỗi ghi được thông báo |
| W12 | 5000 tham chiếu, 32 frame; DPI 100/125/150% | Có thể cuộn frame và trace, không mất nút chính |
| W13 | CLI simulate input/textbook.csv --curve 10 | Summary đúng oracle, đủ trace/curve/input |
| W14 | benchmark_windows.bat | 36 dòng số đo, JSON môi trường và trace_memory.csv |
| W15 | publish_windows.bat; chạy EXE máy Windows x64 | Ứng dụng khởi động, các ca mẫu đúng |

Lưu actual result, screenshot và log. Nghiệm thu smoke tự động không thay thế đánh giá thao tác, DPI và quyền truy cập tệp. Không đánh dấu đã đạt chỉ vì mã nguồn hoặc workflow có sẵn.
