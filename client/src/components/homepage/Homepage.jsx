import { useEffect, useState } from "react";
import { getRecentActivity } from "../../managers/homeManager";
import {
  Card,
  CardBody,
  CardTitle,
  Alert,
  ListGroup,
  ListGroupItem,
  Badge,
} from "reactstrap";
import "./HomePage.css"; // Custom CSS file for additional styling

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
        const monitoredLowStock = (data.lowStockItems || []).filter(
          (item) => item.monitorLowStock
        );
        setLowStockItems(monitoredLowStock);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching recent activity:", err);
        setError("Failed to load recent activity.");
        setLoading(false);
      });
  };

  return (
    <div className="homepage-container">
      <Card className="mt-4 shadow-sm">
        <CardBody>
          <CardTitle tag="h3" className="text-center text-primary mb-4">
            Dashboard
          </CardTitle>
          {loading && <p className="text-center text-muted">Loading data...</p>}
          {error && (
            <Alert color="danger" className="text-center">
              {error}
            </Alert>
          )}
          <div className="activity-section">
            <h4 className="text-secondary">Recent Activity</h4>
            {recentActivity.length > 0 ? (
              <ListGroup className="mt-3">
                {recentActivity.map((item) => (
                  <ListGroupItem key={item.id}>
                    {item.quantity > 0
                      ? `${item.name} was added on ${new Date(
                          item.updatedAt
                        ).toLocaleString()}.`
                      : `${item.name} is low on stock.`}
                  </ListGroupItem>
                ))}
              </ListGroup>
            ) : (
              !loading && (
                <p className="text-muted">No recent activity found.</p>
              )
            )}
          </div>
          <div className="low-stock-section mt-4">
            <h4 className="text-secondary">Low Stock Items</h4>
            {lowStockItems.length > 0 ? (
              <ListGroup className="mt-3">
                {lowStockItems.map((item) => (
                  <ListGroupItem
                    key={item.id}
                    className="d-flex justify-content-between align-items-center"
                  >
                    <span>
                      {item.name} is low on stock with only{" "}
                      <Badge color="danger" pill>
                        {item.quantity}
                      </Badge>{" "}
                      left.
                    </span>
                  </ListGroupItem>
                ))}
              </ListGroup>
            ) : (
              <p className="text-muted">No items are low on stock.</p>
            )}
          </div>
        </CardBody>
      </Card>
    </div>
  );
};
