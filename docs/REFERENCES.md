# Tài liệu tham khảo bản C#

Ngày kiểm tra nguồn trực tuyến: 10/10/2026. Dùng cùng số trích dẫn trong báo cáo.

[1] Tài liệu môn Hệ điều hành do người học cung cấp, Trường Đại học Giao thông Vận tải, Chapter 03 - Memory Management/ch10.pptx, “Chapter 10: Virtual Memory”, slide 15, 22–23, 28–43, 55–62. Mốc oracle: chuỗi slide 33; FIFO 15 ở slide 35, OPT 9 ở slide 37, LRU 12 ở slide 38. Chưa xác minh riêng tên giảng viên/ấn bản.

[2] R. H. Arpaci-Dusseau và A. C. Arpaci-Dusseau, Operating Systems: Three Easy Pieces, chương 22, Beyond Physical Memory: Policies. https://pages.cs.wisc.edu/~remzi/OSTEP/vm-beyondphys-policy.pdf . Đối chiếu vai trò OPT làm chuẩn offline; không sao chép mã.

[3] Microsoft Learn, Windows Forms Overview. https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/ . Căn cứ GUI Windows, controls, sự kiện và vẽ trực tiếp bằng System.Drawing.

[4] Microsoft Learn, Stopwatch Class. https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch . Căn cứ đo elapsed time trong benchmark C#.

[5] Microsoft Learn, GC.GetAllocatedBytesForCurrentThread Method. https://learn.microsoft.com/en-us/dotnet/api/system.gc.getallocatedbytesforcurrentthread . Chỉ số tổng cấp phát trên thread, không phải peak bộ nhớ còn sống hay RSS.

[6] Bốn văn bản yêu cầu do người học gửi: Bài tập lập trình giải thuật hệ điều hành; 6 câu hỏi bắt buộc; Khung báo cáo đề xuất; Nguồn tham khảo và căn cứ thiết kế đề tài.

[7] Microsoft Learn, LinkedList<T> Class. https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.linkedlist-1 . Dùng node để cập nhật thứ tự FIFO/LRU.

[8] Microsoft Learn, NETSDK1100 và EnableWindowsTargeting. https://learn.microsoft.com/en-us/dotnet/core/tools/sdk-errors/netsdk1100 . Phân biệt khả năng cross-build với chạy WinForms trên Windows.

Không đính kèm lại slide gốc trong project. Không sử dụng benchmark hay log Python trước đây như bằng chứng thực thi C#.
