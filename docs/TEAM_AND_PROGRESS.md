# Nhóm và tiến độ

Đây là **kế hoạch đề xuất**, không phải bằng chứng nhóm đã thực hiện. Điền thông tin thật; không gán tên hoặc đóng góp giả.

| Thành viên | MSSV | Vai trò đề xuất | Bằng chứng cần bổ sung |
| --- | --- | --- | --- |
| [Điền tên 1] | [Điền] | Trưởng nhóm; đặc tả; tích hợp | Issue, commit, biên bản review thực tế |
| [Điền tên 2] | [Điền] | FIFO/LRU/OPT/Clock; oracle | Giải thích trace; commit đã sửa/kiểm tra |
| [Điền tên 3] | [Điền] | GUI và CSV | Nghiệm thu Windows, kiểm tra nhập/xuất |
| [Nếu có tên 4] | [Điền] | Test và hiệu năng | Log chạy trên máy nhóm; phân tích số đo |
| [Nếu có tên 5] | [Điền] | Báo cáo và demo | Chỉnh báo cáo; video và phản biện |

Nhóm 3 người có thể gộp test với thuật toán, báo cáo/demo với trưởng nhóm.

| Tuần | Mốc dự kiến | Điều kiện hoàn thành | Bằng chứng thật |
| --- | --- | --- | --- |
| 1 | Đề tài, phạm vi, repo | Thống nhất cold start, page/frame, GUI | [Điền] |
| 2 | Pseudocode, test plan | Oracle slide, Clock và tie rõ ràng | [Điền] |
| 3 | FIFO và LRU | Pass oracle và ca hit cập nhật thứ tự | [Điền] |
| 4 | OPT và Clock | Pass exhaustive OPT; trace R/hand | [Điền] |
| 5 | GUI, CSV, trực quan | Thử nhập/xuất và bước trên Windows | [Điền] |
| 6 | Stress và benchmark | Lưu seed, cấu hình máy, log | [Điền] |
| 7 | Review chéo, báo cáo | Mỗi người giải thích test của người khác | [Điền] |
| 8 | Nộp, video, bảo vệ | Repo sạch, link công khai, kiểm thử lại | [Điền] |

## Contribution statement mẫu cần hoàn thiện

“Tôi là [họ tên, MSSV]. Tôi trực tiếp thực hiện/kiểm tra [phần việc cụ thể], thể hiện qua [commit/issue/log]. Tôi đã dùng AI để [phạm vi hỗ trợ] và tự kiểm chứng bằng [test cụ thể]. Tôi có thể giải thích [thuật toán/ca kiểm thử]. Các phần tôi chưa trực tiếp thực hiện là [nêu rõ].”

Không nộp nguyên văn mẫu chưa điền. Không backdate commit. Nếu đây là mốc bắt đầu muộn so với lịch môn học, ghi tiến độ thật và trao đổi với giảng viên.

## Khai báo AI

ChatGPT hỗ trợ tạo bản đầu mã nguồn, bộ test, tài liệu, dự thảo báo cáo và tổ chức số liệu. Output C# chưa có kết quả thực thi; bộ test và benchmark cần được chạy trên máy có .NET hoạt động. Nhóm cần tự chạy, rà soát, sửa thông tin cá nhân, xác minh giao diện Windows và chịu trách nhiệm về nội dung nộp. Việc nhận bộ project không tự chứng minh mỗi thành viên đã hiểu hoặc đóng góp.
