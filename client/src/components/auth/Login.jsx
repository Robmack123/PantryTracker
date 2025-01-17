import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../../managers/authManager";
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

export default function Login({ setLoggedInUser }) {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [failedLogin, setFailedLogin] = useState(false);

  const handleSubmit = (e) => {
    e.preventDefault();
    login(email, password).then((user) => {
      if (!user) {
        setFailedLogin(true);
      } else {
        setLoggedInUser(user);
        navigate("/");
      }
    });
  };

  return (
    <div className="d-flex justify-content-center align-items-center mt-5">
      <Card className="shadow-sm" style={{ maxWidth: "400px", width: "100%" }}>
        <CardBody>
          <CardTitle tag="h3" className="text-center text-primary mb-4">
            Login
          </CardTitle>
          <Form onSubmit={handleSubmit}>
            <FormGroup>
              <Label for="email">Email</Label>
              <Input
                id="email"
                invalid={failedLogin}
                type="email"
                value={email}
                placeholder="Enter your email"
                onChange={(e) => {
                  setFailedLogin(false);
                  setEmail(e.target.value);
                }}
              />
            </FormGroup>
            <FormGroup>
              <Label for="password">Password</Label>
              <Input
                id="password"
                invalid={failedLogin}
                type="password"
                value={password}
                placeholder="Enter your password"
                onChange={(e) => {
                  setFailedLogin(false);
                  setPassword(e.target.value);
                }}
              />
              {failedLogin && (
                <FormFeedback>Invalid email or password.</FormFeedback>
              )}
            </FormGroup>
            <Button color="primary" block type="submit">
              Login
            </Button>
          </Form>
          <Row className="mt-3 text-center">
            <Col>
              <p className="mb-0">
                Not signed up?{" "}
                <Link to="/register" className="text-primary">
                  Register here
                </Link>
              </p>
            </Col>
          </Row>
        </CardBody>
      </Card>
    </div>
  );
}
