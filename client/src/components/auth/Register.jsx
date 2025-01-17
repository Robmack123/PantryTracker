import { useState } from "react";
import { register } from "../../managers/authManager";
import { Link, useNavigate } from "react-router-dom";
import {
  Button,
  Form,
  FormGroup,
  Input,
  Label,
  FormFeedback,
  Card,
  CardBody,
  CardTitle,
  Row,
  Col,
} from "reactstrap";

export default function Register({ setLoggedInUser }) {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [joinCode, setJoinCode] = useState("");
  const [newHouseholdName, setNewHouseholdName] = useState("");

  const [passwordMismatch, setPasswordMismatch] = useState(false);
  const [registrationFailure, setRegistrationFailure] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      setPasswordMismatch(true);
      return;
    }

    if (joinCode && newHouseholdName) {
      setErrorMessage(
        "Provide either a join code or a new household name, not both."
      );
      return;
    }

    if (!joinCode && !newHouseholdName) {
      setErrorMessage(
        "Provide a join code or a name to create a new household."
      );
      return;
    }

    setErrorMessage("");

    const newUser = {
      firstName,
      lastName,
      email,
      password,
      joinCode: joinCode || null,
      newHouseholdName: newHouseholdName || null,
    };

    try {
      const user = await register(newUser);
      if (user) {
        setLoggedInUser(user);
        navigate("/");
      }
    } catch (err) {
      setRegistrationFailure(true);
      setErrorMessage(err.message || "Registration failed. Please try again.");
    }
  };

  return (
    <div className="d-flex justify-content-center align-items-center mt-5">
      <Card className="shadow-sm" style={{ maxWidth: "500px", width: "100%" }}>
        <CardBody>
          <CardTitle tag="h3" className="text-center text-primary mb-4">
            Sign Up
          </CardTitle>
          <Form onSubmit={handleSubmit}>
            <FormGroup>
              <Label for="firstName">First Name</Label>
              <Input
                id="firstName"
                type="text"
                value={firstName}
                placeholder="Enter your first name"
                onChange={(e) => setFirstName(e.target.value)}
              />
            </FormGroup>
            <FormGroup>
              <Label for="lastName">Last Name</Label>
              <Input
                id="lastName"
                type="text"
                value={lastName}
                placeholder="Enter your last name"
                onChange={(e) => setLastName(e.target.value)}
              />
            </FormGroup>
            <FormGroup>
              <Label for="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={email}
                placeholder="Enter your email"
                onChange={(e) => setEmail(e.target.value)}
              />
            </FormGroup>
            <FormGroup>
              <Label for="password">Password</Label>
              <Input
                id="password"
                invalid={passwordMismatch}
                type="password"
                value={password}
                placeholder="Enter your password"
                onChange={(e) => {
                  setPasswordMismatch(false);
                  setPassword(e.target.value);
                }}
              />
            </FormGroup>
            <FormGroup>
              <Label for="confirmPassword">Confirm Password</Label>
              <Input
                id="confirmPassword"
                invalid={passwordMismatch}
                type="password"
                value={confirmPassword}
                placeholder="Confirm your password"
                onChange={(e) => {
                  setPasswordMismatch(false);
                  setConfirmPassword(e.target.value);
                }}
              />
              {passwordMismatch && (
                <FormFeedback>Passwords do not match!</FormFeedback>
              )}
            </FormGroup>
            <FormGroup>
              <Label for="joinCode">
                Join an Existing Household (Join Code)
              </Label>
              <Input
                id="joinCode"
                type="text"
                value={joinCode}
                placeholder="Enter join code"
                onChange={(e) => setJoinCode(e.target.value)}
                disabled={!!newHouseholdName}
              />
            </FormGroup>
            <FormGroup>
              <Label for="newHouseholdName">
                Create a New Household (Name)
              </Label>
              <Input
                id="newHouseholdName"
                type="text"
                value={newHouseholdName}
                placeholder="Enter household name"
                onChange={(e) => setNewHouseholdName(e.target.value)}
                disabled={!!joinCode}
              />
            </FormGroup>
            {errorMessage && <p className="text-danger">{errorMessage}</p>}
            {registrationFailure && (
              <p className="text-danger">Registration Failure</p>
            )}
            <Button color="primary" block type="submit">
              Register
            </Button>
          </Form>
          <Row className="mt-3 text-center">
            <Col>
              <p className="mb-0">
                Already signed up?{" "}
                <Link to="/login" className="text-primary">
                  Log in here
                </Link>
              </p>
            </Col>
          </Row>
        </CardBody>
      </Card>
    </div>
  );
}
