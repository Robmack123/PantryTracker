import { Table } from "reactstrap";

export const PantryTable = ({
  pantryItems,
  currentPage,
  itemsPerPage,
  onRowClick,
}) => (
  <div className="table-container">
    <Table bordered hover className="table-light table-hover custom-table">
      <thead className="table-primary">
        <tr>
          <th>#</th>
          <th>Name</th>
          <th>Quantity</th>
          <th>Last Updated</th>
        </tr>
      </thead>
      <tbody>
        {pantryItems.map((item, index) => (
          <tr
            key={item.id}
            onClick={() => onRowClick(item)}
            style={{
              cursor: "pointer",
              backgroundColor:
                item.quantity < 2 && item.monitorLowStock
                  ? "#fff6f6"
                  : "inherit",
            }}
          >
            <td>{index + 1 + (currentPage - 1) * itemsPerPage}</td>
            <td>{item.name}</td>
            <td>
              {item.quantity}{" "}
              {item.quantity < 4 && item.monitorLowStock && (
                <span className="badge bg-danger ms-1">Low</span>
              )}
            </td>
            <td>{new Date(item.updatedAt).toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </Table>
  </div>
);
