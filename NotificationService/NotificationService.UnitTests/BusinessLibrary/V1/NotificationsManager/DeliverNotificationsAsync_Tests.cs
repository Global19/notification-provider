﻿// <autogenerated />
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.NotificationsManager_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Moq;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Entities.Web;
    using NUnit.Framework;

    /// <summary>
    /// Test Class.
    /// </summary>
    /// <seealso cref="NotificationService.UnitTests.BusinessLibrary.V1.NotificationsManager_Tests.NotificationsTestsBase" />
    [ExcludeFromCodeCoverage]
    public class DeliverNotificationsAsync_Tests : NotificationsTestsBase
    {
        /// <summary>
        /// Setups the base.
        /// </summary>
        [SetUp]
        public override void SetupBase()
        {
            base.SetupBase();
        }

        /// <summary>
        /// Delivers the notifications asynchronously with invalid application identifier.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="userObjectId">The user object identifier.</param>
        [TestCase(null, "oid")]
        [TestCase("", "oid")]
        [TestCase(" ", "oid")]
        public void DeliverNotificationsAsync_WithInvalidApplicationId(string applicationName, string userObjectId)
        {
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.NotificationManager.DeliverNotificationsAsync(applicationName, userObjectId).ConfigureAwait(false));
            Assert.IsTrue(ex.Message.StartsWith("The application name is not specified.", StringComparison.Ordinal));
        }


        /// <summary>
        /// Delivers the notifications asynchronous with invalid user object identifier.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="userObjectId">The user object identifier.</param>
        [TestCase("appid", null)]
        [TestCase("appid", "")]
        [TestCase("appid", " ")]
        public void DeliverNotificationsAsync_WithInvalidUserObjectId(string applicationName, string userObjectId)
        {
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.NotificationManager.DeliverNotificationsAsync(applicationName, userObjectId).ConfigureAwait(false));
            Assert.IsTrue(ex.Message.StartsWith("The user object identifier is not specified.", StringComparison.Ordinal));
        }

        /// <summary>
        /// Delivers the notifications asynchronous with valid inputs.
        /// </summary>
        [Test]
        public async Task DeliverNotificationsAsync_WithValidInputs()
        {
            EntityCollection<WebNotificationItemEntity> collection = new EntityCollection<WebNotificationItemEntity>();
            string userObjectId = this.NotificationEntities.Where(note => note.Recipient.Name == "P2").Select(note => note.Recipient.ObjectIdentifier).FirstOrDefault();
            collection.Items = this.NotificationEntities.Where(note => note.Recipient.ObjectIdentifier.Equals(userObjectId, StringComparison.OrdinalIgnoreCase) && note.ExpiresOnUTCDate > DateTime.UtcNow && note.PublishOnUTCDate < DateTime.UtcNow);
            _ = this.notificationsRepositoryMock.Setup<Task<EntityCollection<WebNotificationItemEntity>>>(rp => rp.ReadAsync(It.IsAny<Expression<Func<WebNotificationItemEntity, bool>>>(), It.IsAny<Expression<Func<WebNotificationItemEntity, NotificationPriority>>>(), It.IsAny<string>(), 50)).
                Returns(Task.FromResult(collection));
            _ = this.notificationsRepositoryMock.Setup(rp => rp.UpsertAsync(It.IsAny<IEnumerable<WebNotificationItemEntity>>())).ReturnsAsync(collection.Items.Select(it =>
                {
                    it.DeliveredOnChannel = new Dictionary<NotificationDeliveryChannel, bool>
                    {
                        { NotificationDeliveryChannel.Web, true },
                    };
                    return it;
                }));

            var response = await this.NotificationManager.DeliverNotificationsAsync(this.ApplicationName, userObjectId);
            Assert.IsTrue(response.Notifications.Count > 0);
            Assert.IsTrue(response.Notifications.First().ExpiresOnUTCDate > DateTime.UtcNow);
            this.notificationsRepositoryMock.Verify(rp => rp.UpsertAsync(It.IsAny<IEnumerable<WebNotificationItemEntity>>()), Times.Once);
        }
    }
}
