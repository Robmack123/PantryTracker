import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  joinHousehold,
  createHousehold,
} from "../../managers/householdManager";
import {
  Card,
  CardBody,
  CardTitle,
  Form,
  FormGroup,
  Input,
  Button,
  Label,
  Alert,
  Row,
  Col,
} from "reactstrap";

export default function HouseholdSelection({ loggedInUser, setLoggedInUser }) {
  const [joinCode, setJoinCode] = useState("");
  const [newHouseholdName, setNewHouseholdName] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const navigate = useNavigate();

  const handleJoin = () => {
    if (!joinCode) {
      setErrorMessage("Please provide a join code.");
      return;
    }
    joinHousehold(loggedInUser.id, joinCode)
      .then((updatedUser) => {
        setLoggedInUser(updatedUser);
        navigate("/");
      })
      .catch(() => setErrorMessage("Failed to join household. Invalid code."));
  };

  const handleCreate = () => {
    if (!newHouseholdName) {
      setErrorMessage("Please provide a name for the household.");
      return;
    }

    console.log("Sending household name:", newHouseholdName);

    createHousehold(loggedInUser.id, newHouseholdName)
      .then((updatedUser) => {
        setLoggedInUser(updatedUser);
        navigate("/");
      })
      .catch((error) => {
        console.error("Error creating household:", error.message);
        setErrorMessage(error.message);
      });
  };

  return (
    <div className="d-flex justify-content-center align-items-center mt-5">
      <Card className="shadow" style={{ maxWidth: "500px", width: "100%" }}>
        <CardBody>
          <CardTitle tag="h3" className="text-center text-primary mb-4">
            Select or Create a Household
          </CardTitle>

          {errorMessage && <Alert color="danger">{errorMessage}</Alert>}

          <Form>
            <Row>
              <Col xs="12">
                <h5 className="text-secondary">Join an Existing Household</h5>
                <FormGroup>
                  <Label for="joinCode">Join Code</Label>
                  <Input
                    id="joinCode"
                    type="text"
                    placeholder="Enter join code"
                    value={joinCode}
                    onChange={(e) => setJoinCode(e.target.value)}
                  />
                </FormGroup>
                <Button color="primary" block onClick={handleJoin}>
                  Join Household
                </Button>
              </Col>
            </Row>
            <hr />
            <Row>
              <Col xs="12">
                <h5 className="text-secondary">Create a New Household</h5>
                <FormGroup>
                  <Label for="newHouseholdName">Household Name</Label>
                  <Input
                    id="newHouseholdName"
                    type="text"
                    placeholder="Enter household name"
                    value={newHouseholdName}
                    onChange={(e) => setNewHouseholdName(e.target.value)}
                  />
                </FormGroup>
                <Button color="success" block onClick={handleCreate}>
                  Create Household
                </Button>
              </Col>
            </Row>
          </Form>
        </CardBody>
      </Card>
    </div>
  );
}
