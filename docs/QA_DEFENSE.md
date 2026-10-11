# Q1 đến Q6 và chuẩn bị bảo vệ

Core/CSV đạt 25/25 PASS theo log kiểm thử trên Windows ngày 10/10/2026. GUI sau sửa lỗi CS0108 và benchmark cần được nghiệm thu riêng.

## Q1 Bài toán và mô hình

Mô phỏng chuỗi page number của một tiến trình có f frame cố định, bắt đầu trống. Đầu vào là step/page/frames; đầu ra là trace trạng thái, hit/fault, victim, số thay thế và tỷ lệ lỗi. State gồm frames, resident, metadata của chính sách và counters. Page 0 hợp lệ; frame 0 không hợp lệ. Tie OPT chọn frame nhỏ nhất, Clock bắt đầu hand=0. Xem ALGORITHMS.md và báo cáo phần 1.

## Q2 Các bước thuật toán

FIFO loại page nạp sớm nhất; hit không đổi hàng đợi. LRU loại page lâu nhất chưa dùng; hit cập nhật độ mới. OPT loại page có lần dùng kế tiếp xa nhất; không dùng lại được xem là vô cực. Clock quét R từ hand; gặp 1 thì xóa và tiến, gặp 0 thì thay; page mới R=1 và hand tiến. Invariant: page yêu cầu luôn resident sau bước, không trùng page trong frames, số frame cố định. Xem trace tính tay và pseudocode.

## Q3 Bằng chứng đúng

Oracle slide 22 tham chiếu/3 frame cho FIFO 15, LRU 12, OPT 9. Test tính tay Clock kiểm tra từng bit và hand, không chỉ tổng fault. Belady kiểm tra FIFO 9→10. Test page 0, một frame, lặp lại, dữ liệu lỗi; 100 chuỗi ngẫu nhiên có seed đối chiếu oracle list; OPT được so với duyệt mọi khả năng loại trang trên 3276 cặp chuỗi/số frame. Địa chỉ bảo vệ bộ nhớ, race, preemption và deadlock không thuộc mô hình này; không gán các test không liên quan.

## Q4 Thiết kế và truy vết

Core không import Windows Forms. GUI chụp input khi nhấn chạy, Task.Run tính toán và UI thread cập nhật control sau await. CSV xuất state sau từng bước; bảng giữ slot vật lý cố định. So sánh dùng cùng chuỗi, cùng f và cold start cho bốn chính sách. Các file input/output và CLI giúp tái lập không cần GUI. Các thao tác giao diện được kiểm tra theo docs/WINDOWS_ACCEPTANCE.md.

## Q5 Chỉ số và độ phức tạp

Fault rate=F/n; hit rate=H/n; H+F=n; replacement không gồm nạp ban đầu. Số fault đánh giá chính sách; thời gian C# đánh giá cài đặt mô phỏng. FIFO/LRU O(n+f) trung bình theo giả định hash; Clock O(n+f) khấu hao, mỗi fault có thể O(f); OPT O(nf+f), bộ nhớ O(n+f). Trace tăng O(nf). LRU không luôn tốt hơn FIFO trên mọi chuỗi; chuỗi Belady với f=3 có FIFO 9 nhưng LRU 10. OPT là cận dưới trong mô hình. Không suy ra EAT thật khi chưa có chi phí page fault/đĩa thực đo.

## Q6 Nhóm, kiểm thử và giới hạn

Kế hoạch 8 tuần và bảng phân công nằm trong docs/TEAM_AND_PROGRESS.md. Mỗi hạng mục được đối chiếu với kết quả kiểm thử, commit và nội dung báo cáo tương ứng. Benchmark ba workload, ba kích thước, LCG32 seed13, ba warmup, năm lần đo; môi trường lưu JSON. Stress đến 200000 cho cả chuỗi vừa bộ nhớ và chuỗi mọi tham chiếu đều miss. Giới hạn: một tiến trình, không dirty bit/TLB/page size/cấp frame động; GUI bị giới hạn để kiểm soát lượng trace. Có thể cải tiến stream CSV, pagination GUI và thêm I/O cost sau khi thống nhất với giảng viên.

## Câu hỏi ngắn dễ gặp

- Vì sao page fault ở lần đầu vẫn được tính? Page chưa resident, cần nạp dù chưa phải loại victim.
- Vì sao OPT không thể dùng trực tiếp trong OS online? Chưa biết chuỗi truy cập tương lai.
- Vì sao FIFO có Belady? Không có tính chất bao hàm tập resident giữa f và f+1 như LRU/OPT.
- Vì sao Clock có lúc quét cả vòng? Tất cả R đang 1; sau khi xóa hết, vị trí bắt đầu có R=0.
- Vì sao cùng số lỗi chưa đủ? Có thể chọn victim sai nhưng tổng cuối tình cờ giống nhau; cần kiểm tra trace và trạng thái.
- Có chứng minh đây là page replacement của Windows? Không; đây là mô phỏng học thuật chạy bằng ứng dụng Windows.
