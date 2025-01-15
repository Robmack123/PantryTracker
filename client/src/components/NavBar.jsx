import { useState } from "react";
import { NavLink as RRNavLink } from "react-router-dom";
import {
  Navbar,
  NavbarBrand,
  NavbarToggler,
  Collapse,
  Nav,
  NavItem,
  NavLink,
  Button,
} from "reactstrap";
import { logout } from "../managers/authManager";

export default function NavBar({ loggedInUser, setLoggedInUser }) {
  const [isOpen, setIsOpen] = useState(false);

  const toggleNavbar = () => setIsOpen(!isOpen);

  return (
    <Navbar color="light" light expand="md" fixed="top">
      <NavbarBrand tag={RRNavLink} to="/">
        🍴PantryPal
      </NavbarBrand>
      <NavbarToggler onClick={toggleNavbar} />
      <Collapse isOpen={isOpen} navbar>
        <Nav className="me-auto" navbar>
          {loggedInUser && (
            <>
              <NavItem>
                <NavLink tag={RRNavLink} to="/household/manage">
                  Manage Household
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={RRNavLink} to="/pantry">
                  Pantry Items
                </NavLink>
              </NavItem>
            </>
          )}
        </Nav>
        {loggedInUser ? (
          <Button
            color="primary"
            onClick={() => {
              logout().then(() => {
                setLoggedInUser(null);
              });
            }}
          >
            Logout
          </Button>
        ) : (
          <Nav navbar>
            <NavItem>
              <NavLink tag={RRNavLink} to="/login">
                <Button color="primary">Login</Button>
              </NavLink>
            </NavItem>
          </Nav>
        )}
      </Collapse>
    </Navbar>
  );
}
