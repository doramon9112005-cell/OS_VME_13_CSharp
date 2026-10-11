# Mô hình và thuật toán

## Quy ước chung

Một tiến trình, local replacement, f frame có kích thước bằng nhau; trạng thái ban đầu trống. Reference string là page number, không chứa địa chỉ byte. Không mô phỏng dirty bit, TLB, page size, lỗi truy cập địa chỉ bất hợp lệ, chi phí đĩa hoặc cấp frame động. Mọi tham chiếu hợp lệ phải kết thúc với page nằm trong RAM. Một page xuất hiện tối đa một lần trong tập frame.

State = (frames, resident, FIFO/LRU order hoặc Clock R/hand, faults). OPT dùng thêm danh sách lần xuất hiện tương lai. Frame trống nạp từ chỉ số 0 tăng dần. Mỗi fault tăng đếm đúng một lần; chỉ tăng replacements nếu có victim. Hits + faults = n. Frame không có page dùng null, tránh nhầm với page 0.

## FIFO

```text
Q = hàng đợi page theo thứ tự nạp
for page in references:
    nếu page đã resident: HIT, không đổi Q
    ngược lại:
        FAULT
        nếu còn trống: dùng slot trống đầu tiên
        nếu đầy: lấy victim ở đầu Q; dùng slot của victim
        nạp page; thêm page cuối Q
    lưu trạng thái sau bước
```

Q không thay đổi bởi hit. Mọi frame đang dùng có đúng một phần tử trong Q. LinkedList<int> kết hợp Dictionary<int, LinkedListNode<int>> được dùng như queue, tra cứu page→slot bằng Dictionary. Với giả định hash O(1) trung bình: tổng thời gian O(n+f), bộ nhớ phụ O(f), chưa tính input và trace.

## LRU

```text
Q = thứ tự từ ít được truy cập gần đây nhất tới gần đây nhất
for page in references:
    nếu resident: HIT, chuyển page về cuối Q
    ngược lại:
        FAULT
        nếu đầy: lấy đầu Q làm victim
        nạp page vào slot trống hoặc slot victim; thêm cuối Q
```

LRU thay đổi thứ tự cả khi hit. Thời điểm tham chiếu là chỉ số bước tuần tự nên không có hai page khác nhau có cùng thời điểm truy cập cuối. LinkedList.Remove và AddLast cho cập nhật O(1) trung bình; tổng O(n+f), phụ O(f) không tính trace/input.

## OPT

```text
future[p] = hàng đợi mọi vị trí xuất hiện của p trong reference string
for i, page in references:
    bỏ i khỏi đầu future[page]
    nếu resident: HIT
    ngược lại:
        FAULT
        nếu đầy:
            next(p) = đầu future[p], hoặc vô cực nếu không còn dùng
            victim = page có next lớn nhất
            nếu nhiều lựa chọn bằng nhau: chọn frame có chỉ số nhỏ nhất
        nạp page
```

Không tìm lại toàn bộ phần đuôi chuỗi ở mỗi lần fault. Tiền xử lý O(n); tìm victim O(f) cho mỗi fault khi đầy; tổng O(nf+f) worst case, phụ O(n+f). OPT cần biết tương lai nên là chuẩn đối chiếu offline, không phải chính sách online triển khai trực tiếp được.

## Second Chance bằng Clock

```text
hand = 0; R[0..f-1] = 0
for page in references:
    nếu resident:
        HIT; R[slot(page)] = 1; không đổi hand
    ngược lại:
        FAULT
        nếu còn trống: slot = frame trống đầu tiên
        nếu đầy:
            while R[hand] == 1:
                R[hand] = 0
                hand = (hand + 1) mod f
            slot = hand; loại page ở slot
        nạp page vào slot; R[slot] = 1
        hand = (slot + 1) mod f
```

Trang vừa nạp được truy cập ngay nên R=1. Không reset R định kỳ. Nếu mọi R đều 1, quét một vòng xóa tất cả rồi thay tại vị trí bắt đầu. Vòng lặp dừng sau không quá f lần xóa R trong một fault. Một fault có chi phí O(f); trên toàn chuỗi, mỗi lần xóa 1 cần có lần đặt 1 trước đó, mỗi tham chiếu đặt tối đa một lần, nên tổng chi phí quét O(n), tổng O(n+f) khấu hao. Bộ nhớ phụ O(f). Bit R xấp xỉ độ mới, không thể tái tạo đầy đủ thứ tự LRU.

## Trace tính tay cho Clock

Input 1,2,3,2,4,2,5; 3 frame. Tất cả cột là trạng thái **sau** truy cập. F/H = fault/hit.

| Bước | Page | F/H | F0 F1 F2 | R0 R1 R2 | Hand | Victim | Xóa R tại |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 1 | F | 1 – – | 1 0 0 | 1 | – | – |
| 2 | 2 | F | 1 2 – | 1 1 0 | 2 | – | – |
| 3 | 3 | F | 1 2 3 | 1 1 1 | 0 | – | – |
| 4 | 2 | H | 1 2 3 | 1 1 1 | 0 | – | – |
| 5 | 4 | F | 4 2 3 | 1 0 0 | 1 | 1 | 0,1,2 |
| 6 | 2 | H | 4 2 3 | 1 1 0 | 1 | – | – |
| 7 | 5 | F | 4 2 5 | 1 0 1 | 0 | 3 | 1 |

Tổng 5 faults, 2 hits, 2 replacements. Ở bước 7, Clock cho page 2 cơ hội thứ hai; FIFO lại loại page 2. Cùng chuỗi này FIFO, LRU, OPT cũng có 5 faults, nhưng frame cuối khác nhau: FIFO [4,5,3], LRU [4,2,5], OPT [5,2,3]. Số fault giống nhau chưa chứng minh trace đúng.

## Bộ nhớ trace và giới hạn

Bật trace tạo bản sao frames/order/bits cho từng bước, O(nf) thời gian lẫn bộ nhớ; đây là overhead trực quan hóa, không phải độ phức tạp riêng của chọn victim. Không dùng số đo khi trace bật để kết luận tốc độ chính sách thuần. Lõi giới hạn n×f ≤ 1000000 khi trace bật; benchmark chính tắt trace.
