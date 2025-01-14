import { useEffect, useState } from "react";
import { getPantryItems } from "../../managers/pantryItemManager";
import { Card, CardBody, CardTitle, Table, Alert } from "reactstrap";

export const PantryItems = () => {
  const [pantryItems, setPantryItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    getPantryItems()
      .then((items) => {
        setPantryItems(items);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching pantry items:", err);
        setError("Failed to load pantry items.");
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <p>Loading pantry items...</p>;
  }

  if (error) {
    return (
      <Alert color="danger" timeout={3000}>
        {error}
      </Alert>
    );
  }

  return (
    <Card className="mt-4">
      <CardBody>
        <CardTitle tag="h3">Household Pantry Items</CardTitle>
        {pantryItems.length > 0 ? (
          <Table bordered>
            <thead>
              <tr>
                <th>#</th>
                <th>Name</th>
                <th>Quantity</th>
                <th>Last Updated</th>
              </tr>
            </thead>
            <tbody>
              {pantryItems.map((item, index) => (
                <tr key={item.id}>
                  <td>{index + 1}</td>
                  <td>{item.name}</td>
                  <td>{item.quantity}</td>
                  <td>{new Date(item.updatedAt).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <p>No items in the pantry.</p>
        )}
      </CardBody>
    </Card>
  );
};
