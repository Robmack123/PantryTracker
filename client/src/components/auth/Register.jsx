import { useState } from "react";
import { register } from "../../managers/authManager";
import { Link, useNavigate } from "react-router-dom";
import { Button, FormFeedback, FormGroup, Input, Label } from "reactstrap";

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

    // Validate password matching
    if (password !== confirmPassword) {
      setPasswordMismatch(true);
      return;
    }

    // Validate household inputs
    if (joinCode && newHouseholdName) {
      setErrorMessage(
        "Please provide either a join code to join an existing household or a name to create a new household, but not both."
      );
      return;
    }

    if (!joinCode && !newHouseholdName) {
      setErrorMessage(
        "Please provide a join code to join an existing household or a name to create a new household."
      );
      return;
    }

    setErrorMessage("");

    // Create user object
    const newUser = {
      firstName,
      lastName,
      email,
      password,
      joinCode: joinCode || null,
      newHouseholdName: newHouseholdName || null,
    };

    try {
      // Register the user
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
    <div className="container" style={{ maxWidth: "500px" }}>
      <h3>Sign Up</h3>
      <FormGroup>
        <Label>First Name</Label>
        <Input
          type="text"
          value={firstName}
          onChange={(e) => setFirstName(e.target.value)}
        />
      </FormGroup>
      <FormGroup>
        <Label>Last Name</Label>
        <Input
          type="text"
          value={lastName}
          onChange={(e) => setLastName(e.target.value)}
        />
      </FormGroup>
      <FormGroup>
        <Label>Email</Label>
        <Input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
      </FormGroup>
      <FormGroup>
        <Label>Password</Label>
        <Input
          invalid={passwordMismatch}
          type="password"
          value={password}
          onChange={(e) => {
            setPasswordMismatch(false);
            setPassword(e.target.value);
          }}
        />
      </FormGroup>
      <FormGroup>
        <Label>Confirm Password</Label>
        <Input
          invalid={passwordMismatch}
          type="password"
          value={confirmPassword}
          onChange={(e) => {
            setPasswordMismatch(false);
            setConfirmPassword(e.target.value);
          }}
        />
        <FormFeedback>Passwords do not match!</FormFeedback>
      </FormGroup>
      <FormGroup>
        <Label>Join an Existing Household (Join Code)</Label>
        <Input
          type="text"
          value={joinCode}
          onChange={(e) => setJoinCode(e.target.value)}
          disabled={!!newHouseholdName} // Disable if creating a new household
        />
      </FormGroup>
      <FormGroup>
        <Label>Create a New Household (Name)</Label>
        <Input
          type="text"
          value={newHouseholdName}
          onChange={(e) => setNewHouseholdName(e.target.value)}
          disabled={!!joinCode} // Disable if joining an existing household
        />
      </FormGroup>
      <p style={{ color: "red" }} hidden={!errorMessage}>
        {errorMessage}
      </p>
      <p style={{ color: "red" }} hidden={!registrationFailure}>
        Registration Failure
      </p>
      <Button
        color="primary"
        onClick={handleSubmit}
        disabled={passwordMismatch}
      >
        Register
      </Button>
      <p>
        Already signed up? Log in <Link to="/login">here</Link>
      </p>
    </div>
  );
}
