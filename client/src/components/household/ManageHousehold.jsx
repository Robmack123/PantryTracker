import { useEffect, useState } from "react";
import {
  getHouseholdUsers,
  removeUserFromHousehold,
} from "../../managers/householdManager";
import {
  Card,
  CardBody,
  CardTitle,
  CardText,
  Button,
  ListGroup,
  ListGroupItem,
  Alert,
} from "reactstrap";

export default function ManageHousehold() {
  const [household, setHousehold] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [loggedInUserId, setLoggedInUserId] = useState(null); // Store the logged-in user's ID

  useEffect(() => {
    // Fetch household members
    getHouseholdUsers()
      .then((data) => {
        setHousehold(data); // Set the household data
        setLoggedInUserId(data.loggedInUserId); // Set the logged-in user's ID from the API
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching household members:", err);
        setError("Failed to load household members.");
        setLoading(false);
      });
  }, []);

  const handleRemoveUser = (userId) => {
    removeUserFromHousehold(userId)
      .then(() => {
        setHousehold((prevHousehold) => ({
          ...prevHousehold,
          members: prevHousehold.members.filter(
            (member) => member.id !== userId
          ),
        }));
      })
      .catch((err) => {
        console.error("Error removing user:", err);
        setError("Failed to remove user.");
      });
  };

  if (loading) {
    return <p>Loading household members...</p>;
  }

  if (error) {
    return <Alert color="danger">{error}</Alert>;
  }

  return (
    <Card className="mt-4">
      <CardBody>
        <CardTitle tag="h3">Manage Household</CardTitle>
        {household && (
          <>
            <CardText>
              <strong>Join Code:</strong> {household.joinCode}
            </CardText>
            <h4>Members:</h4>
            <ListGroup>
              {household.members?.length > 0 ? (
                household.members.map((member) => (
                  <ListGroupItem
                    key={member.id}
                    className="d-flex justify-content-between align-items-center"
                  >
                    <span>
                      {member.firstName} {member.lastName}
                    </span>
                    {loggedInUserId === household.adminUserId && (
                      <Button
                        color="danger"
                        size="sm"
                        onClick={() => handleRemoveUser(member.id)}
                      >
                        Remove
                      </Button>
                    )}
                  </ListGroupItem>
                ))
              ) : (
                <ListGroupItem>No members in the household.</ListGroupItem>
              )}
            </ListGroup>
          </>
        )}
      </CardBody>
    </Card>
  );
}
