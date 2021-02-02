import * as CBOR from 'cbor';
import { assertionRequest, attestationRequest, CredentialUserInfo } from './authenticatorRequests';
import { buf2b64url, buf2str, getPrintableCopy } from './utils';

export default function useWebAuthn() {
  const createCredential = async (
    user: CredentialUserInfo,
    relyingParty: PublicKeyCredentialRpEntity,
    serverChallenge: string,
    attestation?: AttestationConveyancePreference,
    authenticatorSelection?: AuthenticatorSelectionCriteria,
    excludeCredentials?: string[],
    timeout?: number
  ): Promise<PublicKeyCredential> => {
    console.debug('server challenge: ' + serverChallenge);

    const request = attestationRequest(
      user,
      relyingParty,
      serverChallenge,
      attestation,
      authenticatorSelection,
      excludeCredentials,
      timeout
    );
    console.debug(getPrintableCopy(request));

    const credential = (await navigator.credentials.create({
      publicKey: request,
    })) as PublicKeyCredential;
    console.debug(getPrintableCopy(credential));

    const clientData = JSON.parse(buf2str(credential.response.clientDataJSON));
    console.info(clientData);

    const attestationObject = (credential.response as AuthenticatorAttestationResponse).attestationObject;
    const attestationObjectCBOR = CBOR.decode(attestationObject);
    console.debug(getPrintableCopy(attestationObjectCBOR));

    return credential;
  };

  const requestCredential = async (
    serverChallenge: string,
    verification?: UserVerificationRequirement,
    relyingPartyId?: string,
    allowCredentials?: string[]
  ): Promise<PublicKeyCredential> => {
    console.debug('server challenge: ' + serverChallenge);

    const request = assertionRequest(serverChallenge, verification, relyingPartyId, allowCredentials);
    console.debug(getPrintableCopy(request));

    const credentials = (await navigator.credentials.get({ publicKey: request })) as PublicKeyCredential;
    console.debug(credentials);

    return credentials;
  };

  const encodeCredential = (credential: PublicKeyCredential) => {
    const encoded: any = {
      id: credential.id,
      type: credential.type,
    };

    if (credential.response instanceof AuthenticatorAttestationResponse) {
      const response = credential.response as AuthenticatorAttestationResponse;
      encoded.response = {
        attestationObject: buf2b64url(response.attestationObject),
      };
    }
    if (credential.response instanceof AuthenticatorAssertionResponse) {
      const response = credential.response as AuthenticatorAssertionResponse;
      encoded.response = {
        authenticatorData: buf2b64url(response.authenticatorData),
        signature: buf2b64url(response.signature),
        userHandle: response.userHandle ? buf2b64url(response.userHandle) : null,
      };
    }
    encoded.response.clientDataJSON = buf2b64url(credential.response.clientDataJSON);

    return encoded;
  };

  return {
    createCredential,
    requestCredential,
    encodeCredential,
  };
}
