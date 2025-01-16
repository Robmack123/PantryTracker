import { useEffect, useState } from "react";
import { getRecentActivity } from "../../managers/homeManager";
import { Card, CardBody, CardTitle, Alert } from "reactstrap";

export const HomePage = () => {
  const [recentActivity, setRecentActivity] = useState([]);
  const [lowStockItems, setLowStockItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchRecentActivity();
  }, []);

  const fetchRecentActivity = () => {
    setLoading(true);
    setError("");

    getRecentActivity()
      .then((data) => {
        setRecentActivity((data.recentActivity || []).slice(0, 5)); // Limit to 5 most recent
        setLowStockItems(data.lowStockItems || []);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching recent activity:", err);
        setError("Failed to load recent activity.");
        setLoading(false);
      });
  };

  return (
    <div>
      <Card className="mt-4">
        <CardBody>
          <CardTitle tag="h3">Dashboard</CardTitle>
          {loading && <p>Loading data...</p>}
          {error && (
            <Alert color="danger" timeout={3000}>
              {error}
            </Alert>
          )}
          {/* Recent Activity Section */}
          <div className="mt-3">
            <h4>Recent Activity</h4>
            {recentActivity.length > 0 ? (
              <ul>
                {recentActivity.map((item) => (
                  <li key={item.id}>
                    {item.quantity > 0
                      ? `${item.name} has been added on ${new Date(
                          item.updatedAt
                        ).toLocaleString()}.`
                      : `${item.name} is low on stock.`}
                  </li>
                ))}
              </ul>
            ) : (
              !loading && <p>No recent activity found.</p>
            )}
          </div>
          {/* Low Stock Items Section */}
          <div className="mt-4">
            <h4>Low Stock Items</h4>
            {lowStockItems.length > 0 ? (
              <ul>
                {lowStockItems.map((item) => (
                  <li key={item.id}>
                    {item.name} is low on stock with only {item.quantity} left.
                  </li>
                ))}
              </ul>
            ) : (
              <p>No items are low on stock.</p>
            )}
          </div>
        </CardBody>
      </Card>
    </div>
  );
};
