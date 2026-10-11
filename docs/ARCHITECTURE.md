# Sơ đồ mô hình và module

GitHub có thể hiển thị các khối Mermaid dưới đây khi mở tệp Markdown.

## Luồng xử lý một tham chiếu

```mermaid
flowchart TD
    A["Lấy page tiếp theo"] --> B{"Page resident?"}
    B -->|Có| C["Hit và cập nhật metadata"]
    B -->|Không| D{"Còn frame trống?"}
    D -->|Có| E["Chọn frame trống"]
    D -->|Không| F["Chọn victim theo policy"]
    E --> G["Nạp page và tăng faults"]
    F --> G
    C --> H["Lưu trạng thái sau bước"]
    G --> H
    H --> I{"Còn tham chiếu?"}
    I -->|Có| A
    I -->|Không| J["Trả Result và summary"]
```

Tất cả chính sách dùng cùng quy tắc hit/fault và cold start. Các lựa chọn victim cùng metadata riêng được mô tả tại ALGORITHMS.md.

## Quan hệ module và luồng dữ liệu

```mermaid
flowchart TD
    U["Input tay hoặc CSV"] --> V["CsvFiles validation"]
    V --> G["GUI main thread"]
    V --> C["CLI batch"]
    G --> W["Worker tính toán"]
    W --> K["Core Simulator.Run"]
    C --> K
    T["Tests và benchmark"] --> K
    K --> R["Result và Step"]
    R --> Q["Task hoàn tất"]
    Q -->|await trên UI thread| G
    R --> X["CsvFiles export"]
    G --> D["Frame table và biểu đồ"]
    X --> O["CSV trace và summary"]
```

Worker không gọi WinForms. GUI cập nhật bằng await và Windows Forms Timer trên main thread. Lõi không import GUI hay API tệp; cùng một input có kết quả quyết định giống nhau qua CLI và GUI.
