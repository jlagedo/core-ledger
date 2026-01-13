import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import {
  type RequestAdapter,
  type RequestInformation,
  type Parsable,
  type ParsableFactory,
  type SerializationWriterFactory,
  type ParseNodeFactory,
  type ErrorMappings,
  BackingStoreFactory,
  InMemoryBackingStoreFactory,
} from '@microsoft/kiota-abstractions';
import {
  ParseNodeFactoryRegistry,
  SerializationWriterFactoryRegistry,
} from '@microsoft/kiota-abstractions';
import { JsonParseNodeFactory } from '@microsoft/kiota-serialization-json';
import { JsonSerializationWriterFactory } from '@microsoft/kiota-serialization-json';
import { firstValueFrom } from 'rxjs';

/**
 * Angular HttpClient-based RequestAdapter for Kiota.
 *
 * Integrates Kiota-generated API clients with Angular's HttpClient, ensuring:
 * - Compatibility with Angular HTTP interceptors (auth, mock API, etc.)
 * - Proper error handling through Angular's error pipeline
 * - Type safety with generated DTOs
 *
 * @example
 * ```typescript
 * const adapter = new AngularRequestAdapter(httpClient);
 * adapter.baseUrl = environment.apiUrl;
 * const client = createCoreLedgerApiClient(adapter);
 * ```
 */
export class AngularRequestAdapter implements RequestAdapter {
  public baseUrl = '';

  private readonly parseNodeFactory: ParseNodeFactory;
  private readonly serializationWriterFactory: SerializationWriterFactory;
  private backingStoreFactory: BackingStoreFactory;

  constructor(private readonly httpClient: HttpClient) {
    // Register JSON serialization (default for most APIs)
    const parseNodeRegistry = new ParseNodeFactoryRegistry();
    parseNodeRegistry.contentTypeAssociatedFactories.set(
      'application/json',
      new JsonParseNodeFactory()
    );
    this.parseNodeFactory = parseNodeRegistry;

    const serializationRegistry = new SerializationWriterFactoryRegistry();
    serializationRegistry.contentTypeAssociatedFactories.set(
      'application/json',
      new JsonSerializationWriterFactory()
    );
    this.serializationWriterFactory = serializationRegistry;

    // Default backing store factory
    this.backingStoreFactory = new InMemoryBackingStoreFactory();
  }

  getSerializationWriterFactory(): SerializationWriterFactory {
    return this.serializationWriterFactory;
  }

  getParseNodeFactory(): ParseNodeFactory {
    return this.parseNodeFactory;
  }

  getBackingStoreFactory(): BackingStoreFactory {
    return this.backingStoreFactory;
  }

  /**
   * Sends an HTTP request using Angular's HttpClient
   */
  async send<ModelType extends Parsable>(
    requestInfo: RequestInformation,
    type: ParsableFactory<ModelType>,
    errorMappings: ErrorMappings | undefined
  ): Promise<ModelType | undefined> {
    const response = await this.executeRequest(requestInfo);

    if (!response) {
      return undefined;
    }

    // Check for error status codes
    if (response.status >= 400) {
      throw new Error(`HTTP ${response.status}: ${response.statusText || 'Request failed'}`);
    }

    // Parse response body
    if (!response.body) {
      return undefined;
    }

    const parseNode = this.parseNodeFactory.getRootParseNode('application/json', response.body);
    return parseNode.getObjectValue(type);
  }

  /**
   * Sends a request and returns a collection of parsed objects
   */
  async sendCollection<ModelType extends Parsable>(
    requestInfo: RequestInformation,
    type: ParsableFactory<ModelType>,
    errorMappings: ErrorMappings | undefined
  ): Promise<ModelType[] | undefined> {
    const response = await this.executeRequest(requestInfo);

    if (!response) {
      return undefined;
    }

    // Check for error status codes
    if (response.status >= 400) {
      throw new Error(`HTTP ${response.status}: ${response.statusText || 'Request failed'}`);
    }

    // Parse response body as array
    if (!response.body) {
      return undefined;
    }

    const parseNode = this.parseNodeFactory.getRootParseNode('application/json', response.body);
    return parseNode.getCollectionOfObjectValues(type);
  }

  /**
   * Sends a request and returns primitive value
   */
  async sendPrimitive<ResponseType>(
    requestInfo: RequestInformation,
    responseType:
      | 'string'
      | 'number'
      | 'boolean'
      | 'Date'
      | 'DateOnly'
      | 'TimeOnly'
      | 'Duration'
      | 'ArrayBuffer',
    errorMappings: ErrorMappings | undefined
  ): Promise<ResponseType | undefined> {
    const response = await this.executeRequest(requestInfo);

    if (!response) {
      return undefined;
    }

    // Check for error status codes
    if (response.status >= 400) {
      throw new Error(`HTTP ${response.status}: ${response.statusText || 'Request failed'}`);
    }

    if (!response.body) {
      return undefined;
    }

    // Convert response based on type
    switch (responseType) {
      case 'string':
        return response.body as ResponseType;
      case 'number':
        return Number(response.body) as ResponseType;
      case 'boolean':
        return (response.body === 'true' || response.body === true) as ResponseType;
      case 'Date':
        return new Date(response.body) as ResponseType;
      case 'ArrayBuffer':
        return response.body as ResponseType;
      default:
        return response.body as ResponseType;
    }
  }

  /**
   * Sends a request and returns primitive collection
   */
  async sendCollectionOfPrimitive<ResponseType>(
    requestInfo: RequestInformation,
    responseType: 'string' | 'number' | 'boolean' | 'Date' | 'DateOnly' | 'TimeOnly' | 'Duration',
    errorMappings: ErrorMappings | undefined
  ): Promise<ResponseType[] | undefined> {
    const response = await this.executeRequest(requestInfo);

    if (!response) {
      return undefined;
    }

    // Check for error status codes
    if (response.status >= 400) {
      throw new Error(`HTTP ${response.status}: ${response.statusText || 'Request failed'}`);
    }

    if (!response.body) {
      return undefined;
    }

    const parseNode = this.parseNodeFactory.getRootParseNode('application/json', response.body);
    return parseNode.getCollectionOfPrimitiveValues() as ResponseType[];
  }

  /**
   * Sends a request with no response body expected
   */
  async sendNoResponseContent(
    requestInfo: RequestInformation,
    errorMappings: ErrorMappings | undefined
  ): Promise<void> {
    await this.executeRequest(requestInfo);
  }

  /**
   * Sends a request and returns an enum value
   */
  async sendEnum<EnumObject extends Record<string, unknown>>(
    requestInfo: RequestInformation,
    enumObject: EnumObject,
    errorMappings: ErrorMappings | undefined
  ): Promise<EnumObject[keyof EnumObject] | undefined> {
    const response = await this.executeRequest(requestInfo);

    if (!response) {
      return undefined;
    }

    if (response.status >= 400) {
      throw new Error(`HTTP ${response.status}: ${response.statusText || 'Request failed'}`);
    }

    if (!response.body) {
      return undefined;
    }

    const parseNode = this.parseNodeFactory.getRootParseNode('application/json', response.body);
    return parseNode.getEnumValue(enumObject);
  }

  /**
   * Sends a request and returns a collection of enum values
   */
  async sendCollectionOfEnum<EnumObject extends Record<string, unknown>>(
    requestInfo: RequestInformation,
    enumObject: EnumObject,
    errorMappings: ErrorMappings | undefined
  ): Promise<EnumObject[keyof EnumObject][] | undefined> {
    const response = await this.executeRequest(requestInfo);

    if (!response) {
      return undefined;
    }

    if (response.status >= 400) {
      throw new Error(`HTTP ${response.status}: ${response.statusText || 'Request failed'}`);
    }

    if (!response.body) {
      return undefined;
    }

    const parseNode = this.parseNodeFactory.getRootParseNode('application/json', response.body);
    return parseNode.getCollectionOfEnumValues(enumObject);
  }

  /**
   * Enables backing store support
   */
  enableBackingStore(backingStoreFactory?: BackingStoreFactory | undefined): void {
    this.backingStoreFactory = backingStoreFactory ?? new InMemoryBackingStoreFactory();
  }

  /**
   * Converts URL template to absolute URL
   */
  convertToNativeRequest<T>(requestInfo: RequestInformation): Promise<T> {
    throw new Error('Not implemented - not needed for Angular integration');
  }

  /**
   * Execute HTTP request using Angular's HttpClient
   */
  private async executeRequest(
    requestInfo: RequestInformation
  ): Promise<{ status: number; statusText: string; body: any } | undefined> {
    const url = this.getRequestUrl(requestInfo);
    const method = requestInfo.httpMethod?.toUpperCase() || 'GET';

    // Build headers from Kiota Headers (Map<string, Set<string>>)
    let headersObj: Record<string, string | string[]> = {};
    if (requestInfo.headers) {
      requestInfo.headers.forEach((values: Set<string>, key: string) => {
        const valueArray = Array.from(values);
        headersObj[key] = valueArray.length === 1 ? valueArray[0] : valueArray;
      });
    }
    const headers = new HttpHeaders(headersObj);

    // Build request options
    const options: any = {
      headers,
      observe: 'response',
      responseType: 'json',
    };

    // Get request body if present
    let body: any = undefined;
    if (requestInfo.content) {
      const contentType = headersObj['Content-Type'] as string | undefined;
      if (contentType?.includes('application/json')) {
        // Convert ArrayBuffer to string for JSON
        const decoder = new TextDecoder();
        const uint8Array = new Uint8Array(requestInfo.content);
        body = JSON.parse(decoder.decode(uint8Array));
      } else {
        body = requestInfo.content;
      }
    }

    try {
      // Make request using Angular HttpClient
      let response$;
      switch (method) {
        case 'GET':
          response$ = this.httpClient.get(url, options);
          break;
        case 'POST':
          response$ = this.httpClient.post(url, body, options);
          break;
        case 'PUT':
          response$ = this.httpClient.put(url, body, options);
          break;
        case 'PATCH':
          response$ = this.httpClient.patch(url, body, options);
          break;
        case 'DELETE':
          response$ = this.httpClient.delete(url, options);
          break;
        default:
          throw new Error(`Unsupported HTTP method: ${method}`);
      }

      const response = (await firstValueFrom(response$)) as unknown as HttpResponse<any>;

      return {
        status: response.status,
        statusText: response.statusText,
        body: response.body,
      };
    } catch (error: any) {
      // Re-throw Angular HTTP errors
      throw error;
    }
  }

  /**
   * Builds full request URL from RequestInformation
   */
  private getRequestUrl(requestInfo: RequestInformation): string {
    const pathParameters = requestInfo.pathParameters;
    let url: string = requestInfo.urlTemplate ?? '';

    // Replace {+baseurl} with empty string for relative URLs through Angular proxy.
    // Note: Kiota's generated client overwrites adapter.baseUrl with the OpenAPI server URL
    // if it's empty, so we intentionally ignore this.baseUrl here.
    url = url.replace('{+baseurl}', '');

    // Replace path parameters (excluding baseurl which is already handled)
    if (pathParameters) {
      for (const [key, value] of Object.entries(pathParameters)) {
        if (key !== 'baseurl') {
          url = url.replace(`{${key}}`, encodeURIComponent(String(value)));
        }
      }
    }

    // Remove RFC 6570 URI template query parameter expressions {?param1*,param2*,...}
    url = url.replace(/\{[?&][^}]+\}/g, '');

    // Add query parameters
    const queryParams = requestInfo.queryParameters;
    if (queryParams && Object.keys(queryParams).length > 0) {
      const searchParams = new URLSearchParams();
      for (const [key, value] of Object.entries(queryParams)) {
        if (value !== undefined && value !== null) {
          if (Array.isArray(value)) {
            value.forEach((v) => searchParams.append(key, String(v)));
          } else {
            searchParams.append(key, String(value));
          }
        }
      }
      const queryString = searchParams.toString();
      if (queryString) {
        url += (url.includes('?') ? '&' : '?') + queryString;
      }
    }

    return url;
  }
}
