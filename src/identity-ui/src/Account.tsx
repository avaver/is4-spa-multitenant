import { useCallback, useEffect, useState } from 'react';
import useWebAuthn from './webAuthn/useWebAuthn';
import { arr2b64url, uuidv4 } from './webAuthn/utils';
import { api } from './api';
import Card from 'react-bootstrap/Card';
import Button from 'react-bootstrap/Button';
import Table from 'react-bootstrap/Table';

const Account = () => {
  const [claims, setClaims] = useState<any>({});
  const [tenant, setTenant] = useState<any>({});

  const { createCredential, encodeCredential } = useWebAuthn();

  const isAdmin = useCallback(() => {
    return claims?.tenant_admin === 'true';
  }, [claims?.tenant_admin]);

  const isAdminKeySet = () => {
    return tenant?.adminKeyId;
  };

  const getAdminKey = () => {
    var r = tenant?.keys?.filter((k: any) => k.id === tenant?.adminKeyId);
    return r?.length === 1 ? r[0] : null;
  };

  const registerKey = async (admin: boolean) => {
    const credential = await createCredential(
      { id: uuidv4(), name: admin ? 'admin key' : 'device key', displayName: admin ? 'admin key' : 'device key' },
      { name: 'DentalSuite Nexta', id: 'account.dentalsuite.local' },
      arr2b64url(crypto.getRandomValues(new Uint8Array(32))),
      'direct',
      { userVerification: 'discouraged', authenticatorAttachment: admin ? 'cross-platform' : 'platform' }
    );
    const encodedCredential = encodeCredential(credential);
    console.info(encodedCredential);
    const metadata = await api('/account/key', 'POST', { credential: encodedCredential, admin });
    console.log(metadata);
    const data = await api('/account/tenant');
    setTenant(data);
  };

  // const verifyCredential = async (credId: string) => {
  //   const credential = await requestCredential(
  //     arr2b64url(crypto.getRandomValues(new Uint8Array(32))),
  //     'discouraged',
  //     undefined,
  //     [credId]
  //   );
  //   console.log(credential);
  // };

  const deleteKey = async (id: string) => {
    await api('/account/key/' + id, 'DELETE');
    const data = await api('/account/tenant');
    setTenant(data);
  };

  useEffect(() => {
    (async () => {
      let data = await api('/account/authcookieclaims');
      setClaims(data);
      document.title = data?.name + ' | ' + data?.tenant;
      data = await api('/account/tenant');
      setTenant(data);
    })();
  }, [isAdmin]);

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
          <Table hover>
            <tbody>
              <tr>
                <td>Login name</td>
                <td>{claims.name}</td>
              </tr>
              <tr>
                <td>Email</td>
                <td>{claims.email}</td>
              </tr>
              <tr>
                <td>Phone number</td>
                <td>{claims.phone_number}</td>
              </tr>
              <tr>
                <td>Clinic</td>
                <td>{claims.tenant}</td>
              </tr>
              <tr>
                <td>Clinic admin</td>
                <td className={isAdmin() ? 'text-success' : 'text-danger'}>{isAdmin() ? 'yes' : 'no'}</td>
              </tr>
            </tbody>
          </Table>
        </Card.Body>
      </Card>
      <Card>
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
          <Table striped>
            <tbody>
              <tr>
                <td>{getAdminKey()?.metadataName || 'Admin key'}</td>
                <td>
                  <code>{isAdminKeySet() ? tenant.adminKeyId.substring(0, 20) + '...' : 'not set'}</code>
                </td>
                <td style={{ width: '10%' }}>
                  {isAdmin() && (
                    <Button variant="warning" size="sm" onClick={() => registerKey(true)}>
                      <i className={isAdminKeySet() ? 'bi bi-pencil' : 'bi bi-plus'}></i>
                    </Button>
                  )}
                </td>
              </tr>
              {tenant.keys
                ?.filter((k: any) => k.id !== tenant.adminKeyId)
                .map((k: any) => (
                  <tr key={k.id}>
                    <td>Device key</td>
                    <td>
                      <code>{k.id.substring(0, 20) + '...'}</code>
                    </td>
                    <td>
                      {isAdmin() && (
                        <Button size="sm" variant="danger" onClick={() => deleteKey(k.id)}>
                          <i className="bi bi-trash"></i>
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
            </tbody>
          </Table>
          {isAdmin() && isAdminKeySet() && (
            <Button variant="info" onClick={() => registerKey(false)}>
              Register this device
            </Button>
          )}
        </Card.Body>
      </Card>
    </div>
  );
};

export default Account;
