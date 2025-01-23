import { Table } from "reactstrap";

export const PantryTable = ({ pantryItems, onRowClick }) => {
  // Log pantry items to inspect the data

  return (
    <div className="table-container">
      <Table striped hover className="table-light table-hover custom-table">
        <thead className="table-primary">
          <tr>
            <th>Name</th>
            <th>Quantity</th>
            <th>Last Updated</th>
          </tr>
        </thead>
        <tbody>
          {pantryItems.map((item) => (
            <tr
              key={item.id}
              onClick={() => onRowClick(item)}
              style={{
                cursor: "pointer",
                backgroundColor:
                  item.quantity < (item.lowStockThreshold || 2) &&
                  item.monitorLowStock
                    ? "#fff6f6"
                    : "inherit",
              }}
            >
              <td>{item.name}</td>
              <td>
                {item.quantity}{" "}
                {item.quantity < (item.lowStockThreshold || 2) &&
                  item.monitorLowStock && (
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
};
