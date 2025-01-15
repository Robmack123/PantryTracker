import { useEffect, useState } from "react";
import {
  getPantryItems,
  getPantryItemsByCategory,
} from "../../managers/pantryItemManager";
import { Card, CardBody, CardTitle, Table, Alert } from "reactstrap";
import { CategoryDropdown } from "./CategoryFilter";

export const PantryItems = () => {
  const [pantryItems, setPantryItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchPantryItems();
  }, []);

  const fetchPantryItems = (categoryId) => {
    setLoading(true);
    setError("");

    const fetchFunction = categoryId
      ? () => getPantryItemsByCategory([categoryId])
      : getPantryItems;

    fetchFunction()
      .then((items) => {
        setPantryItems(items);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching pantry items:", err);
        setError("Failed to load pantry items.");
        setLoading(false);
      });
  };

  const handleCategorySelect = (categoryId) => {
    fetchPantryItems(categoryId);
  };

  return (
    <div>
      <Card className="mt-4">
        <CardBody>
          <CardTitle tag="h3">Filter Pantry Items by Category</CardTitle>
          <CategoryDropdown onCategorySelect={handleCategorySelect} />
          {loading && <p>Loading pantry items...</p>}
          {error && (
            <Alert color="danger" timeout={3000}>
              {error}
            </Alert>
          )}
          {pantryItems.length > 0 ? (
            <Table bordered className="mt-3">
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
            !loading && <p>No items found for the selected category.</p>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
