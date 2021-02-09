import { useCallback, useEffect, useState } from 'react';
import useWebAuthn from './webAuthn/useWebAuthn';
import { arr2b64url, uuidv4 } from './webAuthn/utils';
import { api } from './api';
import Card from 'react-bootstrap/Card';
import Button from 'react-bootstrap/Button';
import Table from 'react-bootstrap/Table';
import Modal from 'react-bootstrap/Modal';

const Account = () => {
  const [claims, setClaims] = useState<any>({});
  const [keys, setKeys] = useState<any>([]);
  const [users, setUsers] = useState<any>([]);
  const [showModal, setShowModal] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isAdmin, setIsAdmin] = useState(false);

  const { createCredential, encodeCredential } = useWebAuthn();

  const isClinicAdmin = useCallback(() => claims?.tenant_admin === 'true', [claims?.tenant_admin]);

  const isAdminKeySet = () => keys?.filter((k: any) => k.isAdminKey).length > 0;

  const registerKey = async (isAdminKey: boolean) => {
    const rawCredential = await createCredential(
      {
        id: uuidv4(),
        name: isAdminKey ? 'admin key' : 'device key',
        displayName: isAdminKey ? 'admin key' : 'device key',
      },
      { name: 'DentalSuite Nexta', id: 'account.dentalsuite.local' },
      arr2b64url(crypto.getRandomValues(new Uint8Array(32))),
      'direct',
      { userVerification: 'discouraged', authenticatorAttachment: isAdminKey ? 'cross-platform' : 'platform' }
    );
    const credential = encodeCredential(rawCredential);
    console.info(credential);
    const metadata = await api('/account/keys', 'POST', { credential, isAdminKey });
    console.log(metadata);
    const data = await api('/account/keys');
    setKeys(data);
  };

  const deleteKey = async (id: string) => {
    await api('/account/keys/' + id, 'DELETE');
    const data = await api('/account/keys');
    setKeys(data);
  };

  const setUserLock = async (username: string, isLocked: boolean) => {
    await api('/account/users/' + username, 'PATCH', { isLocked });
    const data = await api('/account/users');
    setUsers(data);
  };

  const createUser = async () => {
    await api('/account/users/', 'POST', { username, password, isAdmin });
    clearUserDialog();
    const data = await api('/account/users');
    setUsers(data);
  };

  const openEditUser = (u: any) => {
    setEditMode(true);
    setUsername(u.username);
    setIsAdmin(u.isAdmin);
    setShowModal(true);
  };

  const editUser = async () => {
    await api('/account/users/' + username, 'PATCH', { password, isAdmin });
    clearUserDialog();
    const data = await api('/account/users');
    setUsers(data);
  };

  const clearUserDialog = () => {
    setShowModal(false);
    setUsername('');
    setPassword('');
    setIsAdmin(false);
  };

  useEffect(() => {
    (async () => {
      let data = await api('/account/authcookieclaims');
      setClaims(data);
      document.title = data?.name + ' | ' + data?.tenant;

      data = await api('/account/keys');
      setKeys(data);

      if (isClinicAdmin()) {
        data = await api('/account/users');
        setUsers(data);
      }
    })();
  }, [isClinicAdmin]);

  return (
    <div className="container">
      <Card className="mb-3">
        <Card.Body>
          <Card.Title className="mb-5">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="48"
              height="48"
              fill="currentColor"
              className="bi bi-file-earmark-person"
              viewBox="0 0 16 16"
            >
              <path d="M11 8a3 3 0 1 1-6 0 3 3 0 0 1 6 0z" />
              <path d="M14 14V4.5L9.5 0H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2zM9.5 3A1.5 1.5 0 0 0 11 4.5h2v9.255S12 12 8 12s-5 1.755-5 1.755V2a1 1 0 0 1 1-1h5.5v2z" />
            </svg>
            <span className="ml-3">Account Information</span>
            <Button variant="info" className="float-right" onClick={() => (window.location.href = '/logout')}>
              Sign out
            </Button>
          </Card.Title>
          <Table>
            <tbody>
              <tr>
                <td>Login name</td>
                <td>{claims?.name}</td>
              </tr>
              <tr>
                <td>Clinic</td>
                <td>{claims?.tenant}</td>
              </tr>
              <tr>
                <td>Clinic admin</td>
                <td className={isClinicAdmin() ? 'text-success' : 'text-danger'}>{isClinicAdmin() ? 'yes' : 'no'}</td>
              </tr>
            </tbody>
          </Table>
        </Card.Body>
      </Card>
      <Card className="mb-3">
        <Card.Body>
          <Card.Title className="mb-5">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="48"
              height="48"
              fill="currentColor"
              className="bi bi-key"
              viewBox="0 0 16 16"
            >
              <path d="M0 8a4 4 0 0 1 7.465-2H14a.5.5 0 0 1 .354.146l1.5 1.5a.5.5 0 0 1 0 .708l-1.5 1.5a.5.5 0 0 1-.708 0L13 9.207l-.646.647a.5.5 0 0 1-.708 0L11 9.207l-.646.647a.5.5 0 0 1-.708 0L9 9.207l-.646.647A.5.5 0 0 1 8 10h-.535A4 4 0 0 1 0 8zm4-3a3 3 0 1 0 2.712 4.285A.5.5 0 0 1 7.163 9h.63l.853-.854a.5.5 0 0 1 .708 0l.646.647.646-.647a.5.5 0 0 1 .708 0l.646.647.646-.647a.5.5 0 0 1 .708 0l.646.647.793-.793-1-1h-6.63a.5.5 0 0 1-.451-.285A3 3 0 0 0 4 5z" />
              <path d="M4 8a1 1 0 1 1-2 0 1 1 0 0 1 2 0z" />
            </svg>
            <span className="ml-3">Admin and device keys</span>
          </Card.Title>
          <Table hover>
            <tbody>
              {keys
                ?.sort((a: any, b: any) => (a.isAdminKey === b.isAdminKey ? 0 : a.isAdminKey ? -1 : 1))
                .map((k: any) => (
                  <tr key={k.id}>
                    <td>{k.isAdminKey ? 'Admin Key' : 'Device key'}</td>
                    <td>
                      <code>{k.id.substring(0, 20) + '...'}</code>
                    </td>
                    <td style={{ width: '20%' }} className="text-center">
                      {isClinicAdmin() && (
                        <Button size="sm" variant="danger" onClick={() => deleteKey(k.id)}>
                          <i className="bi bi-trash"></i>
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
            </tbody>
          </Table>
          {isClinicAdmin() && (
            <Button variant="warning" onClick={() => registerKey(true)}>
              Register Admin Key
            </Button>
          )}
          {isClinicAdmin() && isAdminKeySet() && (
            <Button variant="info" className="ml-3" onClick={() => registerKey(false)}>
              Register This Device
            </Button>
          )}
        </Card.Body>
      </Card>
      {isClinicAdmin() && (
        <Card>
          <Card.Body>
            <Card.Title className="mb-5">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="48"
                height="48"
                fill="currentColor"
                className="bi bi-people-fill"
                viewBox="0 0 16 16"
              >
                <path d="M7 14s-1 0-1-1 1-4 5-4 5 3 5 4-1 1-1 1H7zm4-6a3 3 0 1 0 0-6 3 3 0 0 0 0 6z" />
                <path
                  fillRule="evenodd"
                  d="M5.216 14A2.238 2.238 0 0 1 5 13c0-1.355.68-2.75 1.936-3.72A6.325 6.325 0 0 0 5 9c-4 0-5 3-5 4s1 1 1 1h4.216z"
                />
                <path d="M4.5 8a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z" />
              </svg>
              <span className="ml-3">Clinic Users</span>
            </Card.Title>
            <Table hover>
              <tbody>
                {users
                  .sort((a: any, b: any) =>
                    a.isAdmin === b.isAdmin ? a.username.localeCompare(b.username) : a.isAdmin ? -1 : 1
                  )
                  .map((u: any) => (
                    <tr key={u.username}>
                      <td>{u.username}</td>
                      <td>{u.isAdmin && <span className="text-success">admin</span>}</td>
                      <td style={{ width: '20%' }} className="text-center">
                        <Button size="sm" variant="warning" onClick={() => openEditUser(u)}>
                          <i className="bi bi-pencil"></i>
                        </Button>
                        <Button
                          size="sm"
                          variant={u.isLocked ? 'danger' : 'success'}
                          className="ml-3"
                          disabled={u.username === claims.name}
                          onClick={() => setUserLock(u.username, !u.isLocked)}
                        >
                          <i className={u.isLocked ? 'bi bi-lock' : 'bi bi-unlock'}></i>
                        </Button>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </Table>
            {isClinicAdmin() && (
              <Button
                variant="warning"
                onClick={() => {
                  setEditMode(false);
                  setShowModal(true);
                }}
              >
                Create User
              </Button>
            )}
          </Card.Body>
        </Card>
      )}
      <Modal
        show={showModal}
        onHide={() => setShowModal(false)}
        animation={false}
        centered
        backdrop="static"
        keyboard={false}
      >
        <form>
          <Modal.Body>
            <div className="form-group mt-3">
              <input
                type="text"
                className="form-control"
                id="username"
                placeholder="Username (initials)"
                autoComplete="username"
                required
                readOnly={editMode}
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
            </div>
            <div className="form-group">
              <input
                type="password"
                className="form-control"
                id="password"
                placeholder={editMode ? 'New password (leave blank to keep current password)' : 'Password'}
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
            <div className="form-group custom-control custom-switch">
              <input
                type="checkbox"
                className="custom-control-input"
                id="isadmin"
                checked={isAdmin}
                onChange={() => setIsAdmin(!isAdmin)}
              />
              <label className="custom-control-label" htmlFor="isadmin">
                Clinic Administrator
              </label>
            </div>
            <div className="text-right">
              <button
                className="btn btn-primary"
                type="submit"
                onClick={(e) => {
                  e.preventDefault();
                  editMode ? editUser() : createUser();
                }}
              >
                Save
              </button>
              <Button variant="danger" className="ml-3" onClick={() => clearUserDialog()}>
                Cancel
              </Button>
            </div>
          </Modal.Body>
        </form>
      </Modal>
    </div>
  );
};

export default Account;
